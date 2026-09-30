using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PersonalBrand.Core;

namespace PersonalBrand.Infrastructure;

public sealed class AudienceService(BrandDbContext db, TimeProvider clock, IOptions<SiteOptions> site) : IAudienceService
{
    internal async Task<Contact> GetContactAsync(ParticipantInput input, CancellationToken ct)
    {
        input.Validate();
        var normalized = Contact.NormalizeEmail(input.Email);
        // Transaction-scoped lock also covers an email that has no row yet.
        var resource = "contact:" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(normalized)));
        await SqlLocks.AcquireAsync(db, resource, ct);
        var contact = await db.Contacts.SingleOrDefaultAsync(x => x.NormalizedEmail == normalized, ct);
        if (contact is null)
        {
            contact = new Contact
            {
                FullName = input.FullName.Trim(),
                Mobile = input.Mobile,
                NormalizedMobile = input.Mobile is null ? null : new string(input.Mobile.Where(char.IsDigit).ToArray()),
                TelegramUsername = input.TelegramUsername,
                PreferredLanguage = input.Language,
                CreatedAt = clock.GetUtcNow(),
                UpdatedAt = clock.GetUtcNow()
            };
            contact.SetEmail(input.Email);
            db.Contacts.Add(contact);
        }

        // Anonymous requests cannot overwrite an existing person's identity details.
        contact.UpdatedAt = clock.GetUtcNow();
        return contact;
    }

    internal async Task ConsentAsync(Contact contact, ParticipantInput input, string source, CancellationToken ct)
    {
        if (!input.MarketingConsent)
            return;
        foreach (var type in Enum.GetValues<ConsentType>())
            db.Consents.Add(new ConsentRecord { ContactId = contact.Id, ConsentType = type, Granted = true, GrantedAt = clock.GetUtcNow(), Source = source });
        var subscription = await db.Subscriptions.SingleOrDefaultAsync(x => x.ContactId == contact.Id, ct);
        if (subscription is null)
        {
            subscription = new Subscription
            {
                ContactId = contact.Id,
                CreatedAt = clock.GetUtcNow()
            };
            db.Subscriptions.Add(subscription);
        }
        else
            subscription.Activate();
        Queue(contact.Email, "Your update preferences", $"You opted in to educational updates. Unsubscribe at {site.Value.BaseUrl}/{input.Language}/unsubscribe/{subscription.UnsubscribeToken}");
    }

    internal void Queue(string recipient, string subject, string body, string channel = "email") => db.Notifications.Add(new Notification { Recipient = recipient, Subject = subject, Body = body, Channel = channel, CreatedAt = clock.GetUtcNow() });
    public async Task EnrollAsync(Guid course, ParticipantInput input, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        if (!await db.Courses.AnyAsync(x => x.Id == course && x.IsVisible && x.Status == ContentStatus.Published && x.Language == input.Language, ct))
            throw new DomainException("Course is unavailable.");
        var contact = await GetContactAsync(input, ct);
        if (!await db.Enrollments.AnyAsync(x => x.CourseId == course && x.ContactId == contact.Id, ct))
        {
            db.Enrollments.Add(new CourseEnrollment { CourseId = course, ContactId = contact.Id, EnrolledAt = clock.GetUtcNow(), Language = input.Language, MarketingConsent = input.MarketingConsent, MarketingConsentAt = input.MarketingConsent ? clock.GetUtcNow() : null });
            Queue(contact.Email, "Course enrollment", "You are enrolled. Lessons are freely available without a login.");
        }

        await ConsentAsync(contact, input, "course", ct);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task SubscribeAsync(ParticipantInput input, CancellationToken ct)
    {
        if (!input.MarketingConsent)
            throw new DomainException("Please explicitly consent to receive updates.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var contact = await GetContactAsync(input, ct);
        await ConsentAsync(contact, input, "newsletter", ct);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task<bool> UnsubscribeAsync(string token, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var subscription = await db.Subscriptions.SingleOrDefaultAsync(x => x.UnsubscribeToken == token, ct);
        if (subscription is null)
            return false;
        var contact = await db.Contacts.SingleAsync(x => x.Id == subscription.ContactId, ct);
        var resource = "contact:" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(contact.NormalizedEmail)));
        await SqlLocks.AcquireAsync(db, resource, ct);
        subscription.Unsubscribe(clock.GetUtcNow());
        await db.Consents.Where(x => x.ContactId == subscription.ContactId && x.Granted && x.RevokedAt == null).ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, clock.GetUtcNow()), ct);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return true;
    }
}

internal static class SqlLocks
{
    public static Task AcquireAsync(BrandDbContext db, string resource, CancellationToken ct) => db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @r int; EXEC @r = sys.sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=15000; IF @r < 0 THROW 51000, 'Resource is busy. Please retry.', 1;", ct);
}
