using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Admin.Pages;

public sealed class SettingsModel(BrandDbContext db) : PageModel
{
    public static readonly string[] Pages = ["about", "resume", "now", "contact", "privacy"];
    public static readonly string[] Keys = new[]
    {
        "Payment.BankName",
        "Payment.CardHolderName",
        "Payment.CardNumberDisplay",
        "Payment.Instructions.en",
        "Payment.Instructions.fa"
    }.Concat(Pages.SelectMany(p => new[] { $"Page.{p}.en", $"Page.{p}.fa" })).ToArray();
    [BindProperty]
    public Dictionary<string, string?> Values { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Values = await db.Settings.Where(x => Keys.Contains(x.Key)).ToDictionaryAsync(x => x.Key, x => (string?)x.Value, ct);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        foreach (var key in Keys)
        {
            var value = Values.GetValueOrDefault(key) ?? "";
            if (key.StartsWith("Page.") && string.IsNullOrWhiteSpace(new SafeMarkdown().PlainText(value)))
                ModelState.AddModelError("", key + ": content is required in both languages.");
            if (value.Length > 20000) ModelState.AddModelError("", key + ": text is too long.");
        }
        if (!ModelState.IsValid) return Page();
        foreach (var key in Keys)
        {
            var item = await db.Settings.SingleOrDefaultAsync(x => x.Key == key, ct);
            if (item is null)
            {
                item = new SiteSetting
                {
                    Key = key
                };
                db.Settings.Add(item);
            }

            item.Value = Values.GetValueOrDefault(key) ?? "";
        }

        await db.SaveChangesAsync(ct);
        TempData["Message"] = "Settings saved.";
        return RedirectToPage();
    }
}
