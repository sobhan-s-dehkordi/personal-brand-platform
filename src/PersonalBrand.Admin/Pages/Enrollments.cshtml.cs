using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Admin.Pages;

public sealed class EnrollmentsModel(BrandDbContext db) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid? Course
    {
        get; set;
    }

    [BindProperty(SupportsGet = true)]
    public string? Language
    {
        get; set;
    }

    [BindProperty(SupportsGet = true)]
    public DateTimeOffset? Since
    {
        get; set;
    }

    [BindProperty(SupportsGet = true)]
    public int Number { get; set; } = 1;
    public List<CourseEnrollment> Items { get; set; } = [];
    public List<CourseChoice> Courses { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Number = Math.Clamp(Number, 1, 10000);
        Courses = await db.Courses.AsNoTracking().OrderBy(x => x.Title).Select(x => new CourseChoice(x.Id, x.Title)).Take(500).ToListAsync(ct);
        Items = await db.Enrollments.Include(x => x.Contact).Include(x => x.Course).AsNoTracking().Where(x => (Course == null || x.CourseId == Course) && (Language == null || x.Language == Language) && (Since == null || x.EnrolledAt >= Since)).OrderByDescending(x => x.EnrolledAt).Skip((Number - 1) * 25).Take(25).ToListAsync(ct);
    }
}

public record CourseChoice(Guid Id, string Title);
