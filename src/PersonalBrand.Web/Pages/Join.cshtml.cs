using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Web.Pages;

public sealed class JoinModel(BrandDbContext db, IAudienceService audience, IReservationService reservations) : PublicPage
{
    [BindProperty]
    public ParticipantInput Input { get; set; } = new();

    [BindProperty]
    public string? Website
    {
        get; set;
    }
    public Content Item { get; set; } = null!;
    public bool Workshop => Item is Workshop;

    public async Task<IActionResult> OnGetAsync(string section, string slug, string action, CancellationToken ct)
    {
        Content? item = section == "courses" && action == "enroll" ? await db.Courses.AsNoTracking().SingleOrDefaultAsync(x => x.Slug == slug && x.Language == Culture && x.IsVisible && x.Status == ContentStatus.Published, ct) : section == "workshops" && action == "reserve" ? await db.Workshops.AsNoTracking().SingleOrDefaultAsync(x => x.Slug == slug && x.Language == Culture && x.IsVisible && x.Status == ContentStatus.Published, ct) : null;
        if (item is null)
            return NotFound();
        Item = item;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string section, string slug, string action, CancellationToken ct)
    {
        var result = await OnGetAsync(section, slug, action, ct);
        if (result is NotFoundResult)
            return result;
        if (!string.IsNullOrEmpty(Website))
            return BadRequest();
        Input.Language = Culture;
        if (!ModelState.IsValid)
            return Page();
        try
        {
            if (Workshop)
            {
                var reservation = await reservations.ReserveAsync(Item.Id, Input, ct);
                return RedirectToPage("/Reservation", new
                {
                    Culture,
                    token = reservation.AccessToken
                });
            }

            await audience.EnrollAsync(Item.Id, Input, ct);
            TempData["Message"] = T("You're enrolled. Enjoy the lessons!", "ثبت‌نام شما انجام شد. از درس‌ها لذت ببرید!");
            return Redirect($"/{Culture}/courses/{slug}");
        }
        catch (DomainException e)
        {
            ModelState.AddModelError("", e.Message);
            return Page();
        }
    }
}
