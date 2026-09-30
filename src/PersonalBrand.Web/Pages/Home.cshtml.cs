using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Web.Pages;

public sealed class HomeModel(BrandDbContext db, IAudienceService audience, SiteContent content) : PublicPage
{
    public string Text(string key) => content.Get(key, Culture);
    public List<Project> Projects { get; set; } = [];
    public List<Article> Articles { get; set; } = [];
    public List<Course> Courses { get; set; } = [];
    public List<Workshop> Workshops { get; set; } = [];

    [BindProperty]
    public ParticipantInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        await content.LoadAsync(ct);
        Projects = await db.Projects.AsNoTracking().Where(x => x.Language == Culture && x.IsVisible && x.Status == ContentStatus.Published).OrderByDescending(x => x.IsFeatured).ThenBy(x => x.DisplayOrder).Take(3).ToListAsync(ct);
        Articles = await db.Articles.AsNoTracking().Where(x => x.Language == Culture && x.IsVisible && x.Status == ContentStatus.Published).OrderByDescending(x => x.PublishedAt).Take(3).ToListAsync(ct);
        Courses = await db.Courses.AsNoTracking().Where(x => x.Language == Culture && x.IsVisible && x.Status == ContentStatus.Published).OrderByDescending(x => x.IsFeatured).Take(3).ToListAsync(ct);
        Workshops = await db.Workshops.AsNoTracking().Where(x => x.Language == Culture && x.IsVisible && x.Status == ContentStatus.Published && x.WorkshopStatus == WorkshopStatus.Open).OrderBy(x => x.StartDate).Take(3).ToListAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await content.LoadAsync(ct);
        Input.Language = Culture;
        if (string.IsNullOrWhiteSpace(Input.FullName))
        {
            Input.FullName = Text("Home.SubscriberName");
            ModelState.Remove("Input.FullName");
        }

        if (ModelState.IsValid)
        {
            try
            {
                await audience.SubscribeAsync(Input, ct);
                TempData["Message"] = Text("Home.Subscribed");
                return RedirectToPage(new
                {
                    Culture
                });
            }
            catch (DomainException e)
            {
                ModelState.AddModelError("", e.Message);
            }
        }

        return await OnGetAsync(ct);
    }
}
