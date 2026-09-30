using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Admin.Pages;

public sealed class AudienceModel(BrandDbContext db) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Kind { get; set; } = "Contacts";

    [BindProperty(SupportsGet = true)]
    public int Number { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? Q
    {
        get; set;
    }

    [BindProperty(SupportsGet = true)]
    public string? Language
    {
        get; set;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? Course
    {
        get; set;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? Workshop
    {
        get; set;
    }

    [BindProperty(SupportsGet = true)]
    public bool? Consent
    {
        get; set;
    }

    [BindProperty(SupportsGet = true)]
    public DateTimeOffset? Since
    {
        get; set;
    }

    [BindProperty(SupportsGet = true)]
    public SubscriptionStatus? Subscription
    {
        get; set;
    }
    public List<CourseChoice> Courses { get; set; } = [];
    public List<CourseChoice> Workshops { get; set; } = [];
    public List<AudienceRow> Items { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Number = Math.Clamp(Number, 1, 10000);
        Courses = await db.Courses.AsNoTracking().OrderBy(x => x.Title).Select(x => new CourseChoice(x.Id, x.Title)).Take(500).ToListAsync(ct);
        Workshops = await db.Workshops.AsNoTracking().OrderBy(x => x.Title).Select(x => new CourseChoice(x.Id, x.Title)).Take(500).ToListAsync(ct);
        var contacts = db.Contacts.AsNoTracking().Where(x => (Q == null || x.FullName.Contains(Q) || x.Email.Contains(Q)) && (Language == null || x.PreferredLanguage == Language) && (Since == null || x.CreatedAt >= Since));
        if (Course != null)
            contacts = contacts.Where(x => db.Enrollments.Any(e => e.ContactId == x.Id && e.CourseId == Course));
        if (Workshop != null)
            contacts = contacts.Where(x => db.Reservations.Any(r => r.ContactId == x.Id && r.WorkshopId == Workshop));
        if (Consent != null)
            contacts = contacts.Where(x => db.Consents.Any(c => c.ContactId == x.Id && c.Granted && c.RevokedAt == null) == Consent);
        if (Kind == "Enrollments")
            contacts = contacts.Where(x => db.Enrollments.Any(e => e.ContactId == x.Id));
        if (Kind == "Subscribers")
            contacts = contacts.Where(x => db.Subscriptions.Any(s => s.ContactId == x.Id));
        if (Subscription != null)
            contacts = contacts.Where(x => db.Subscriptions.Any(s => s.ContactId == x.Id && s.Status == Subscription));
        Items = await contacts.OrderByDescending(x => x.CreatedAt).Skip((Number - 1) * 25).Take(25).Select(x => new AudienceRow(x.Id, x.FullName, x.Email, x.PreferredLanguage, x.CreatedAt, db.Enrollments.Count(e => e.ContactId == x.Id), db.Reservations.Count(r => r.ContactId == x.Id), db.Subscriptions.Where(s => s.ContactId == x.Id).Select(s => (SubscriptionStatus?)s.Status).FirstOrDefault())).ToListAsync(ct);
    }
}

public record AudienceRow(Guid Id, string Name, string Email, string Language, DateTimeOffset Created, int Enrollments, int Reservations, SubscriptionStatus? Subscription);
