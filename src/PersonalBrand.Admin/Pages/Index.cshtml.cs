using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Admin.Pages;

public sealed class IndexModel(BrandDbContext db, TimeProvider clock) : PageModel
{
    public Dictionary<string, int> Metrics { get; } = [];
    public List<WorkshopMetric> Workshops { get; set; } = [];
    public List<WorkshopReservation> RecentReservations { get; set; } = [];
    public int UnsentNotifications
    {
        get; set;
    }

    public async Task OnGetAsync(CancellationToken ct)
    {
        RecentReservations = await db.Reservations.Include(x => x.Contact).Include(x => x.Workshop).AsNoTracking().OrderByDescending(x => x.ReservedAt).Take(5).ToListAsync(ct);
        UnsentNotifications = await db.Notifications.CountAsync(x => x.SentAt == null, ct);
        Metrics["Published articles"] = await db.Articles.CountAsync(x => x.Language == "en" && (x.Status == ContentStatus.Published || db.Articles.Any(p => p.TranslationGroupId == x.TranslationGroupId && p.Status == ContentStatus.Published)), ct);
        Metrics["Published courses"] = await db.Courses.CountAsync(x => x.Language == "en" && (x.Status == ContentStatus.Published || db.Courses.Any(p => p.TranslationGroupId == x.TranslationGroupId && p.Status == ContentStatus.Published)), ct);
        Metrics["Reservations"] = await db.Reservations.CountAsync(ct);
        Metrics["Pending payment"] = await db.Reservations.CountAsync(x => x.Status == ReservationStatus.PendingPayment && x.HoldExpiresAt > clock.GetUtcNow(), ct);
        Metrics["Awaiting review"] = await db.Reservations.CountAsync(x => x.Status == ReservationStatus.PaymentSubmitted, ct);
        Metrics["Confirmed participants"] = await db.Reservations.CountAsync(x => x.Status == ReservationStatus.Confirmed, ct);
        Metrics["Course enrollments"] = await db.Enrollments.CountAsync(ct);
        Metrics["Active subscribers"] = await db.Subscriptions.CountAsync(x => x.Status == SubscriptionStatus.Active, ct);
        Metrics["Contacts this month"] = await db.Contacts.CountAsync(x => x.CreatedAt > clock.GetUtcNow().AddDays(-30), ct);
        var now = clock.GetUtcNow();
        var english = await db.Workshops.AsNoTracking().Where(x => x.Language == "en" && (x.StartDate > now || db.Workshops.Any(p => p.TranslationGroupId == x.TranslationGroupId && p.StartDate > now))).OrderBy(x => x.StartDate).Take(12).ToListAsync(ct);
        var groups = english.Select(x => x.TranslationGroupId).ToArray();
        var persian = await db.Workshops.AsNoTracking().Where(x => x.Language == "fa" && groups.Contains(x.TranslationGroupId)).ToListAsync(ct);
        var versions = english.Concat(persian).ToList();
        var ids = versions.Select(x => x.Id).ToArray();
        var counts = await db.Reservations.Where(x => ids.Contains(x.WorkshopId)).GroupBy(x => x.WorkshopId)
            .Select(g => new { Id = g.Key, Confirmed = g.Count(r => r.Status == ReservationStatus.Confirmed), Held = g.Count(r => r.Status == ReservationStatus.PaymentSubmitted || r.Status == ReservationStatus.PendingPayment && r.HoldExpiresAt > now) }).ToDictionaryAsync(x => x.Id, ct);
        WorkshopMetric Metric(Workshop w) => new(w.Id, w.Title, w.Capacity, counts.GetValueOrDefault(w.Id)?.Confirmed ?? 0, counts.GetValueOrDefault(w.Id)?.Held ?? 0);
        Workshops = english.Select(w =>
        {
            var metric = Metric(w);
            var fa = persian.SingleOrDefault(p => p.TranslationGroupId == w.TranslationGroupId);
            metric.Persian = fa is null ? null : Metric(fa);
            return metric;
        }).ToList();
    }
}

public record WorkshopMetric(Guid Id, string Title, int Capacity, int Confirmed, int Held)
{
    public WorkshopMetric? Persian
    {
        get; set;
    }
}
