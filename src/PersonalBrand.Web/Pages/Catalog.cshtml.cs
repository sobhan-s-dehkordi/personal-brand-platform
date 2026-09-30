using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Web.Pages;

public sealed class CatalogModel(BrandDbContext db) : PublicPage
{
    [BindProperty(SupportsGet = true)]
    public string Section { get; set; } = "articles";

    [BindProperty(SupportsGet = true)]
    public string? Q
    {
        get; set;
    }

    [BindProperty(SupportsGet = true)]
    public string? Tag
    {
        get; set;
    }

    [BindProperty(SupportsGet = true)]
    public int Number { get; set; } = 1;
    public List<ContentCard> Items { get; set; } = [];
    public bool HasNext
    {
        get; set;
    }
    public string? EmptyMessage
    {
        get; set;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Number = Math.Clamp(Number, 1, 10000);
        Items = Section switch
        {
            "articles" => await Read(db.Articles.Where(x => Tag == null || x.Tags.Any(t => t.Name == Tag)), ct),
            "courses" => await Read(db.Courses, ct),
            "projects" => await Read(db.Projects, ct),
            _ => await Read(db.Workshops, ct)
        };
        HasNext = Items.Count > 12;
        Items = Items.Take(12).ToList();
        if (Items.Count == 0 && string.IsNullOrWhiteSpace(Q) && string.IsNullOrWhiteSpace(Tag) && Number == 1)
            EmptyMessage = T("New work will appear here soon.", "مطالب تازه به‌زودی اینجا منتشر می‌شود.");
        return Page();
    }

    private Task<List<ContentCard>> Read<T>(IQueryable<T> query, CancellationToken ct)
        where T : Content => query.AsNoTracking().Where(x => x.Language == Culture && x.IsVisible && x.Status == ContentStatus.Published && (Q == null || x.Title.Contains(Q) || x.Summary.Contains(Q))).OrderByDescending(x => x.PublishedAt).ThenBy(x => x.Id).Skip((Number - 1) * 12).Take(13).Select(x => new ContentCard(x.Slug, x.Title, x.Summary, x.CoverImage)).ToListAsync(ct);
}

public record ContentCard(string Slug, string Title, string Summary, string? Image);
