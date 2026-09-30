using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Infrastructure;
using PersonalBrand.Core;

namespace PersonalBrand.Admin.Pages;

public sealed class HomeContentModel(BrandDbContext db) : PageModel
{
    [BindProperty]
    public Dictionary<string, string?> Values { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Values = await db.Settings.ToDictionaryAsync(x => x.Key, x => (string?)x.Value, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        foreach (var language in new[] { "en", "fa" })
            foreach (var field in SiteContent.Fields)
            {
                var value = Values.GetValueOrDefault(field.Key + "." + language) ?? "";
                if (!string.IsNullOrEmpty(field.English) && string.IsNullOrWhiteSpace(new SafeMarkdown().PlainText(value)))
                    ModelState.AddModelError("", (language == "en" ? "English " : "Persian ") + field.Label + " is required.");
                if (value.Length > 20000)
                    ModelState.AddModelError("", field.Label + ": Text is too long.");
                if ((field.Key.EndsWith("Url") || field.Key is "Brand.ProfileImage" or "Brand.ResumePath") && !string.IsNullOrWhiteSpace(value) && !((value.StartsWith("/") && !value.StartsWith("//") && !value.Contains('\\')) || Uri.TryCreate(value, UriKind.Absolute, out var url) && url.Scheme == "https"))
                    ModelState.AddModelError("", field.Label + ": Use a valid HTTPS URL or a path on this site.");
                if (field.Key == "Brand.ContactEmail" && !string.IsNullOrWhiteSpace(value) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(value))
                    ModelState.AddModelError("", "Enter a valid contact email.");
            }

        if (!ModelState.IsValid)
            return Page();
        var records = await db.Settings.ToDictionaryAsync(x => x.Key, ct);
        foreach (var language in new[] { "en", "fa" })
            foreach (var field in SiteContent.Fields)
            {
                var key = field.Key + "." + language;
                if (!records.TryGetValue(key, out var item))
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
        TempData["Message"] = "Homepage content saved.";
        return RedirectToPage();
    }
}
