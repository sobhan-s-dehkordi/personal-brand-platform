using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Admin.Pages;

public sealed class ReservationsModel(BrandDbContext db) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid? Workshop
    {
        get; set;
    }

    [BindProperty(SupportsGet = true)]
    public ReservationStatus? Status
    {
        get; set;
    }

    [BindProperty(SupportsGet = true)]
    public string? Q
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
    public List<WorkshopReservation> Items { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Number = Math.Clamp(Number, 1, 10000);
        Items = await db.Reservations.Include(x => x.Contact).Include(x => x.Workshop).AsNoTracking().Where(x => (Workshop == null || x.WorkshopId == Workshop) && (Status == null || x.Status == Status) && (Since == null || x.ReservedAt >= Since) && (Q == null || x.Contact.Email.Contains(Q) || x.Contact.FullName.Contains(Q) || x.PublicReference.Contains(Q))).OrderByDescending(x => x.ReservedAt).Skip((Number - 1) * 25).Take(25).ToListAsync(ct);
    }
}
