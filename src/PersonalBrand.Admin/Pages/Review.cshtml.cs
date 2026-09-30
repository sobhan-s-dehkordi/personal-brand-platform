using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Admin.Pages;

public sealed class ReviewModel(BrandDbContext db, IReservationService service, IFileStorage storage) : PageModel
{
    public WorkshopReservation Item { get; set; } = null!;

    [BindProperty, StringLength(2000)]
    public string Note { get; set; } = "";

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        var item = await db.Reservations.Include(x => x.Contact).Include(x => x.Workshop).Include(x => x.Receipts).Include(x => x.History).AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item is null)
            return NotFound();
        Item = item;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, string action, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await service.ReviewAsync(id, action, User.FindFirstValue(ClaimTypes.NameIdentifier)!, Note, ct);
                TempData["Message"] = "Reservation updated.";
                return RedirectToPage(new
                {
                    id
                });
            }
            catch (DomainException e)
            {
                ModelState.AddModelError("", e.Message);
            }
        }

        return await OnGetAsync(id, ct);
    }

    public async Task<IActionResult> OnGetReceiptAsync(Guid id, Guid receipt, CancellationToken ct)
    {
        var file = await db.Receipts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == receipt && x.ReservationId == id, ct);
        if (file is null)
            return NotFound();
        Response.Headers.CacheControl = "no-store";
        return File(await storage.OpenPrivateAsync(file.StorageKey, ct), file.ContentType);
    }
}
