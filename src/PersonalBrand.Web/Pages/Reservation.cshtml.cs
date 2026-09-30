using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Web.Pages;

[RequestSizeLimit(6 * 1024 * 1024)]
public sealed class ReservationModel(BrandDbContext db, IReservationService service, TimeProvider clock) : PublicPage
{
    public WorkshopReservation Item { get; set; } = null!;
    public Dictionary<string, string> Payment { get; set; } = [];
    public bool AcceptsPayment => Item.Status == ReservationStatus.PendingPayment && Item.HoldExpiresAt > clock.GetUtcNow();

    [BindProperty, Required, StringLength(80)]
    public string Tracking { get; set; } = "";

    [BindProperty, Required]
    public IFormFile? Receipt
    {
        get; set;
    }

    public async Task<IActionResult> OnGetAsync(string token, CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        var item = await db.Reservations.Include(x => x.Workshop).AsNoTracking().SingleOrDefaultAsync(x => x.AccessToken == token, ct);
        if (item is null)
            return NotFound();
        Item = item;
        Payment = await db.Settings.Where(x => x.Key.StartsWith("Payment.")).ToDictionaryAsync(x => x.Key, x => x.Value, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string token, CancellationToken ct)
    {
        var result = await OnGetAsync(token, ct);
        if (result is NotFoundResult)
            return result;
        if (!ModelState.IsValid || Receipt is null)
            return Page();
        try
        {
            await using var stream = Receipt.OpenReadStream();
            await service.SubmitAsync(token, Tracking, stream, Receipt.FileName, ct);
            return RedirectToPage(new
            {
                Culture,
                token
            });
        }
        catch (DomainException e)
        {
            ModelState.AddModelError("", e.Message);
            return Page();
        }
    }
}
