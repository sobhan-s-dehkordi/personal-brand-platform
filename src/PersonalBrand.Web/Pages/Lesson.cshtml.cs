using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Web.Pages;

public sealed class LessonModel(BrandDbContext db) : PublicPage
{
    public Lesson Item { get; set; } = null!;
    public List<Lesson> Navigation { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(string courseSlug, string lessonSlug, CancellationToken ct)
    {
        var item = await db.Lessons.Include(x => x.Course).AsNoTracking().SingleOrDefaultAsync(x => x.Course.Language == Culture && x.Course.Slug == courseSlug && x.Course.IsVisible && x.Course.Status == ContentStatus.Published && x.Slug == lessonSlug && x.IsPublished, ct);
        if (item is null)
            return NotFound();
        Item = item;
        Navigation = await db.Lessons.AsNoTracking().Where(x => x.CourseId == item.CourseId && x.IsPublished).OrderBy(x => x.Order).ToListAsync(ct);
        return Page();
    }
}
