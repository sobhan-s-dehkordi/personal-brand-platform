using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Web.Pages;

public sealed class DetailModel(BrandDbContext db, TimeProvider clock) : PublicPage
{
    public Content Item { get; set; } = null!;
    public List<Article> Related { get; set; } = [];
    public int Remaining
    {
        get; set;
    }
    public bool CanReserve
    {
        get; set;
    }
    public string Section { get; set; } = "";

    public async Task<IActionResult> OnGetAsync(string section, string slug, CancellationToken ct)
    {
        Section = section;
        Content? item = section switch
        {
            "articles" => await db.Articles.Include(x => x.Tags).Include(x => x.RelatedCourses).AsNoTracking().SingleOrDefaultAsync(x => x.Language == Culture && x.Slug == slug && x.IsVisible && x.Status == ContentStatus.Published, ct),
            "courses" => await db.Courses.Include(x => x.Lessons.Where(l => l.IsPublished)).AsNoTracking().SingleOrDefaultAsync(x => x.Language == Culture && x.Slug == slug && x.IsVisible && x.Status == ContentStatus.Published, ct),
            "projects" => await db.Projects.Include(x => x.Technologies).Include(x => x.Screenshots).AsNoTracking().SingleOrDefaultAsync(x => x.Language == Culture && x.Slug == slug && x.IsVisible && x.Status == ContentStatus.Published, ct),
            "workshops" => await db.Workshops.Include(x => x.Sessions.Where(s => s.IsVisible)).AsNoTracking().SingleOrDefaultAsync(x => x.Language == Culture && x.Slug == slug && x.IsVisible && x.Status == ContentStatus.Published, ct),
            _ => null
        };
        if (item is null)
            return NotFound();
        Item = item;
        if (item is Workshop w)
        {
            var occupied = await ReservationService.Active(db.Reservations.Where(x => x.WorkshopId == w.Id), clock.GetUtcNow()).CountAsync(ct);
            Remaining = Math.Max(0, w.Capacity - occupied);
            CanReserve = w.CanReserve(clock.GetUtcNow(), occupied);
        }

        if (item is Article)
            Related = await db.Articles.AsNoTracking().Where(x => x.Language == Culture && x.IsVisible && x.Status == ContentStatus.Published && x.Id != item.Id).OrderByDescending(x => x.PublishedAt).Take(3).ToListAsync(ct);
        if (item.TranslationGroupId is not null)
        {
            var translation = section switch
            {
                "articles" => await Translation(db.Articles, ct),
                "courses" => await Translation(db.Courses, ct),
                "projects" => await Translation(db.Projects, ct),
                _ => await Translation(db.Workshops, ct)
            };
            if (translation is not null)
                ViewData["TranslationPath"] = $"/{(Fa ? "en" : "fa")}/{section}/{translation}";
        }

        return Page();
    }

    private Task<string?> Translation<T>(IQueryable<T> query, CancellationToken ct)
        where T : Content => query.Where(x => x.TranslationGroupId == Item.TranslationGroupId && x.Language != Culture && x.IsVisible && x.Status == ContentStatus.Published).Select(x => x.Slug).FirstOrDefaultAsync(ct);
}
