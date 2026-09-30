using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PersonalBrand.Core;

namespace PersonalBrand.Infrastructure;

public sealed class ReservationService(BrandDbContext db, AudienceService audience, TimeProvider clock, IOptions<WorkshopReservationOptions> options, IOptions<SiteOptions> site, IFileStorage storage) : IReservationService
{
    public static IQueryable<WorkshopReservation> Active(IQueryable<WorkshopReservation> query, DateTimeOffset now) => query.Where(x => x.Status == ReservationStatus.Confirmed || x.Status == ReservationStatus.PaymentSubmitted || x.Status == ReservationStatus.PendingPayment && x.HoldExpiresAt > now);
    private Task<Workshop> LockWorkshop(Guid id, CancellationToken ct) => db.Workshops.FromSqlInterpolated($"SELECT * FROM Workshops WITH (UPDLOCK, HOLDLOCK) WHERE Id = {id}").SingleAsync(ct);
    public async Task<WorkshopReservation> ReserveAsync(Guid workshop, ParticipantInput input, CancellationToken ct)
    {
        input.Validate();
        if (string.IsNullOrWhiteSpace(input.Mobile))
            throw new DomainException("A mobile number is required for workshop logistics.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var item = await LockWorkshop(workshop, ct);
        var now = clock.GetUtcNow();
        var count = await Active(db.Reservations.Where(x => x.WorkshopId == workshop), now).CountAsync(ct);
        if (item.Language != input.Language || !item.CanReserve(now, count))
            throw new DomainException("Registration is closed or all seats are currently reserved.");
        var contact = await audience.GetContactAsync(input, ct);
        if (await Active(db.Reservations.Where(x => x.WorkshopId == workshop && x.ContactId == contact.Id), now).AnyAsync(ct))
            throw new DomainException("An active reservation already exists. Use the private link sent to your email or contact support.");
        var reservation = WorkshopReservation.Create(workshop, contact.Id, item.Price, item.Currency, now, TimeSpan.FromMinutes(options.Value.HoldDurationMinutes));
        db.Reservations.Add(reservation);
        await audience.ConsentAsync(contact, input, "workshop", ct);
        audience.Queue(contact.Email, "Workshop reservation", $"Reference: {reservation.PublicReference}\nAmount: {item.Price} {item.Currency}\nHold expires (UTC): {reservation.HoldExpiresAt:u}\nPrivate payment instructions: {site.Value.BaseUrl}/{input.Language}/reservations/{reservation.AccessToken}");
        audience.Queue("admin", "Reservation", $"New reservation {reservation.PublicReference}." + (count + 1 == item.Capacity ? " Workshop is now full." : ""), "telegram");
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return reservation;
    }

    public async Task SubmitAsync(string token, string tracking, Stream stream, string name, CancellationToken ct)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(tracking, "^[a-zA-Z0-9-]{4,80}$"))
            throw new DomainException("Enter a valid payment tracking number.");
        var stored = await storage.StoreAsync(stream, name, false, ct);
        try
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            var workshop = await db.Reservations.Where(x => x.AccessToken == token).Select(x => x.WorkshopId).SingleAsync(ct);
            await LockWorkshop(workshop, ct);
            var reservation = await db.Reservations.Include(x => x.Contact).SingleAsync(x => x.AccessToken == token, ct);
            reservation.SubmitPayment(clock.GetUtcNow());
            reservation.Receipts.Add(new PaymentReceipt { ReservationId = reservation.Id, TrackingNumber = tracking, StorageKey = stored.Key, ContentType = stored.ContentType, FileSize = stored.Length, OriginalFileName = Path.GetFileName(name)[..Math.Min(Path.GetFileName(name).Length, 200)], UploadedAt = clock.GetUtcNow() });
            audience.Queue(reservation.Contact.Email, "Payment received for review", $"Receipt for {reservation.PublicReference} is awaiting review.");
            audience.Queue("admin", "Payment submitted", $"Review payment for {reservation.PublicReference} in the admin portal.", "telegram");
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await storage.DeletePrivateAsync(stored.Key, CancellationToken.None);
            throw;
        }
    }

    public async Task ReviewAsync(Guid id, string action, string admin, string note, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var workshopId = await db.Reservations.Where(x => x.Id == id).Select(x => x.WorkshopId).SingleAsync(ct);
        await LockWorkshop(workshopId, ct);
        var item = await db.Reservations.Include(x => x.Contact).Include(x => x.Workshop).ThenInclude(x => x.Sessions).Include(x => x.Receipts).SingleAsync(x => x.Id == id, ct);
        var now = clock.GetUtcNow();
        switch (action)
        {
            case "confirm":
                item.Confirm(now, admin);
                break;
            case "reject":
                item.Reject(now, admin, note);
                break;
            case "cancel":
                item.Cancel(now, admin, note);
                break;
            default:
                throw new DomainException("Unknown action.");
        }

        item.InternalNotes = note;
        foreach (var receipt in item.Receipts)
        {
            receipt.ReviewedAt = now;
            receipt.ReviewedByAdminId = admin;
            receipt.ReviewNotes = note;
        }

        var schedule = string.Join("\n", item.Workshop.Sessions.OrderBy(x => x.SessionNumber).Select(x => $"{x.Title}: {x.StartAt:u} UTC"));
        var joining = item.Status == ReservationStatus.Confirmed ? "\nJoining details: " + new SafeMarkdown().PlainText(item.Workshop.Platform) + "\n" + schedule : "";
        audience.Queue(item.Contact.Email, $"Reservation {item.Status}", $"{item.PublicReference}: {item.Status}. Contact support with any questions.{joining}");
        audience.Queue("admin", "Reservation reviewed", $"Reservation {item.PublicReference}: {item.Status}.", "telegram");
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
