namespace PersonalBrand.Core;

public sealed class WorkshopReservation : Entity
{
    public string PublicReference { get; private set; } = "WS-" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();
    public string AccessToken { get; private set; } = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
    public Guid WorkshopId
    {
        get; set;
    }
    public Workshop Workshop { get; set; } = null!;
    public Guid ContactId
    {
        get; set;
    }
    public Contact Contact { get; set; } = null!;
    public ReservationStatus Status
    {
        get; private set;
    }
    public DateTimeOffset ReservedAt
    {
        get; private set;
    }
    public DateTimeOffset HoldExpiresAt
    {
        get; private set;
    }
    public DateTimeOffset? PaymentSubmittedAt
    {
        get; private set;
    }
    public DateTimeOffset? ConfirmedAt
    {
        get; private set;
    }
    public DateTimeOffset? CancelledAt
    {
        get; private set;
    }
    public decimal PriceAtReservationTime
    {
        get; private set;
    }
    public string Currency { get; private set; } = "IRR";
    public string InternalNotes { get; set; } = "";
    public byte[] RowVersion { get; set; } = [];
    public List<PaymentReceipt> Receipts { get; set; } = [];
    public List<ReservationHistory> History { get; set; } = [];

    private WorkshopReservation()
    {
    }

    public static WorkshopReservation Create(Guid workshop, Guid contact, decimal price, string currency, DateTimeOffset now, TimeSpan hold)
    {
        if (hold <= TimeSpan.Zero || price < 0)
            throw new DomainException("Invalid reservation terms.");
        var item = new WorkshopReservation
        {
            WorkshopId = workshop,
            ContactId = contact,
            PriceAtReservationTime = price,
            Currency = currency,
            ReservedAt = now,
            HoldExpiresAt = now + hold
        };
        item.Record("Created", now, "Visitor");
        return item;
    }

    public bool HoldsSeat(DateTimeOffset now) => Status is ReservationStatus.Confirmed or ReservationStatus.PaymentSubmitted || Status == ReservationStatus.PendingPayment && HoldExpiresAt > now;
    public void SubmitPayment(DateTimeOffset now)
    {
        if (Status != ReservationStatus.PendingPayment || now >= HoldExpiresAt)
            throw new DomainException("This reservation is no longer accepting payment. Contact support if you have transferred funds.");
        Status = ReservationStatus.PaymentSubmitted;
        PaymentSubmittedAt = now;
        Record("Payment submitted", now, "Visitor");
    }

    public void Confirm(DateTimeOffset now, string admin)
    {
        RequireSubmitted();
        Status = ReservationStatus.Confirmed;
        ConfirmedAt = now;
        Record("Confirmed", now, admin);
    }

    public void Reject(DateTimeOffset now, string admin, string note)
    {
        RequireSubmitted();
        Status = ReservationStatus.Rejected;
        Record("Rejected", now, admin, note);
    }

    public void Cancel(DateTimeOffset now, string actor, string note)
    {
        if (Status is not (ReservationStatus.PendingPayment or ReservationStatus.PaymentSubmitted or ReservationStatus.Confirmed))
            throw new DomainException("This reservation cannot be cancelled.");
        Status = ReservationStatus.Cancelled;
        CancelledAt = now;
        Record("Cancelled", now, actor, note);
    }

    public void Expire(DateTimeOffset now)
    {
        if (Status != ReservationStatus.PendingPayment || HoldExpiresAt > now)
            throw new DomainException("This reservation has not expired.");
        Status = ReservationStatus.Expired;
        Record("Expired", now, "System");
    }

    private void RequireSubmitted()
    {
        if (Status != ReservationStatus.PaymentSubmitted)
            throw new DomainException("Only submitted payments can be reviewed.");
    }

    private void Record(string action, DateTimeOffset now, string actor, string? note = null) => History.Add(new ReservationHistory { ReservationId = Id, Action = action, OccurredAt = now, ActorType = actor is "Visitor" or "System" ? actor : "Admin", ActorIdentifier = actor, Note = note });
}
