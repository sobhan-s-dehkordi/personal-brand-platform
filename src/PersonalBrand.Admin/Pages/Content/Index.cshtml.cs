using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Admin.Pages.Content;

public sealed class IndexModel(BrandDbContext db) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Kind { get; set; } = "Articles";

    [BindProperty(SupportsGet = true)]
    public int Number { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? Q
    {
        get; set;
    }
    public List<ContentRow> Items { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Number = Math.Clamp(Number, 1, 10000);
        Items = Kind switch
        {
            "Articles" => await Read(db.Articles, ct),
            "Courses" => await Read(db.Courses, ct),
            "Projects" => await Read(db.Projects, ct),
            _ => await Read(db.Workshops, ct)
        };
    }

    private Task<List<ContentRow>> Read<T>(IQueryable<T> query, CancellationToken ct)
        where T : PersonalBrand.Core.Content => query.AsNoTracking()
            .Where(x => x.Language == "en" && (Q == null || x.Title.Contains(Q) || query.Any(p => p.TranslationGroupId == x.TranslationGroupId && p.Language == "fa" && p.Title.Contains(Q))))
            .OrderByDescending(x => x.UpdatedAt).Skip((Number - 1) * 25).Take(25)
            .Select(x => new ContentRow(x.Id, x.Title, x.Status.ToString(), x.IsVisible,
                query.Where(p => p.TranslationGroupId == x.TranslationGroupId && p.Language == "fa").Select(p => (Guid?)p.Id).SingleOrDefault(),
                query.Any(p => p.TranslationGroupId == x.TranslationGroupId && p.Language == "fa" && p.IsVisible)))
            .ToListAsync(ct);
}

public record ContentRow(Guid Id, string Title, string Status, bool EnglishVisible, Guid? PersianId, bool PersianVisible);
