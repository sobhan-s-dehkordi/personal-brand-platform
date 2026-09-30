using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Admin.Pages;

public sealed class ContactModel(BrandDbContext db) : PageModel
{
    public Contact Item { get; set; } = null!;
    public List<CourseEnrollment> Enrollments { get; set; } = [];
    public List<WorkshopReservation> Reservations { get; set; } = [];
    public List<ConsentRecord> Consents { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        var item = await db.Contacts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item is null)
            return NotFound();
        Item = item;
        Enrollments = await db.Enrollments.Include(x => x.Course).AsNoTracking().Where(x => x.ContactId == id).OrderByDescending(x => x.EnrolledAt).Take(100).ToListAsync(ct);
        Reservations = await db.Reservations.Include(x => x.Workshop).AsNoTracking().Where(x => x.ContactId == id).OrderByDescending(x => x.ReservedAt).Take(100).ToListAsync(ct);
        Consents = await db.Consents.AsNoTracking().Where(x => x.ContactId == id).OrderByDescending(x => x.GrantedAt).Take(100).ToListAsync(ct);
        return Page();
    }
}
