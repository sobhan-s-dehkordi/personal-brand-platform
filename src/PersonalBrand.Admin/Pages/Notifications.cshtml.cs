using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Infrastructure;
using PersonalBrand.Core;

namespace PersonalBrand.Admin.Pages;

public sealed class NotificationsModel(BrandDbContext db, Microsoft.Extensions.Options.IOptions<EmailOptions> email, Microsoft.Extensions.Options.IOptions<TelegramOptions> telegram) : PageModel
{
    public bool EmailConfigured => !string.IsNullOrWhiteSpace(email.Value.Host) && !string.IsNullOrWhiteSpace(email.Value.From);
    public bool TelegramConfigured => !string.IsNullOrWhiteSpace(telegram.Value.BotToken) && !string.IsNullOrWhiteSpace(telegram.Value.ChatId);
    public List<Notification> Items { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Items = await db.Notifications.AsNoTracking().OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(ct);
    }

    public async Task<IActionResult> OnPostRetryAsync(Guid id, CancellationToken ct)
    {
        await db.Notifications.Where(x => x.Id == id && x.SentAt == null).ExecuteUpdateAsync(s => s.SetProperty(x => x.Attempts, 0).SetProperty(x => x.NextAttemptAt, (DateTimeOffset?)null), ct);
        return RedirectToPage();
    }
}
