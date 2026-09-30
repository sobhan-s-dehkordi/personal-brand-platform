using PersonalBrand.Core;

namespace PersonalBrand.UnitTests;

public class DomainTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 28, 12, 0, 0, TimeSpan.Zero);
    private static WorkshopReservation Reservation() => WorkshopReservation.Create(Guid.NewGuid(), Guid.NewGuid(), 100, "IRR", Now, TimeSpan.FromHours(2));
    [Fact]
    public void Pending_hold_expires_at_exact_deadline()
    {
        var r = Reservation();
        Assert.True(r.HoldsSeat(Now.AddMinutes(119)));
        Assert.False(r.HoldsSeat(Now.AddHours(2)));
        r.Expire(Now.AddHours(2));
        Assert.Equal(ReservationStatus.Expired, r.Status);
    }

    [Fact]
    public void Submitted_payment_keeps_seat_until_review()
    {
        var r = Reservation();
        r.SubmitPayment(Now.AddMinutes(5));
        Assert.True(r.HoldsSeat(Now.AddDays(1)));
        r.Confirm(Now.AddDays(1), "admin");
        Assert.True(r.HoldsSeat(Now.AddDays(2)));
        Assert.Equal(3, r.History.Count);
    }

    [Fact]
    public void Cannot_submit_after_expiration()
    {
        var r = Reservation();
        Assert.Throws<DomainException>(() => r.SubmitPayment(Now.AddHours(2)));
    }

    [Fact]
    public void Cannot_confirm_unpaid_or_rejected_reservation()
    {
        var r = Reservation();
        Assert.Throws<DomainException>(() => r.Confirm(Now, "admin"));
        r.SubmitPayment(Now);
        r.Reject(Now, "admin", "Not received");
        Assert.False(r.HoldsSeat(Now));
        Assert.Throws<DomainException>(() => r.Confirm(Now, "admin"));
    }

    [Fact]
    public void Cancellation_releases_seat()
    {
        var r = Reservation();
        r.Cancel(Now, "admin", "Cancelled");
        Assert.False(r.HoldsSeat(Now));
        Assert.Throws<DomainException>(() => r.Cancel(Now, "admin", "Again"));
    }

    [Fact]
    public void Cannot_expire_early_or_submitted()
    {
        var r = Reservation();
        Assert.Throws<DomainException>(() => r.Expire(Now));
        r.SubmitPayment(Now);
        Assert.Throws<DomainException>(() => r.Expire(Now.AddDays(1)));
    }

    [Fact]
    public void Contact_normalizes_email()
    {
        var c = new Contact();
        c.SetEmail(" Person@Example.com ");
        Assert.Equal("PERSON@EXAMPLE.COM", c.NormalizedEmail);
        Assert.Throws<DomainException>(() => c.SetEmail("invalid"));
    }

    [Fact]
    public void Consent_is_not_selected_by_default()
    {
        Assert.False(new ParticipantInput().MarketingConsent);
    }

    [Fact]
    public void Subscription_has_secure_unique_token_and_unsubscribes()
    {
        var a = new Subscription();
        var b = new Subscription();
        Assert.Equal(64, a.UnsubscribeToken.Length);
        Assert.NotEqual(a.UnsubscribeToken, b.UnsubscribeToken);
        a.Unsubscribe(Now);
        Assert.Equal(SubscriptionStatus.Unsubscribed, a.Status);
        Assert.Equal(Now, a.UnsubscribedAt);
    }

    [Fact]
    public void Availability_respects_window_status_and_capacity()
    {
        var w = new Workshop
        {
            Status = ContentStatus.Published,
            WorkshopStatus = WorkshopStatus.Open,
            Capacity = 7,
            RegistrationOpensAt = Now,
            RegistrationClosesAt = Now.AddDays(1)
        };
        Assert.True(w.CanReserve(Now, 6));
        Assert.False(w.CanReserve(Now, 7));
        Assert.False(w.CanReserve(Now.AddDays(1), 0));
        w.WorkshopStatus = WorkshopStatus.Cancelled;
        Assert.False(w.CanReserve(Now, 0));
    }
}
