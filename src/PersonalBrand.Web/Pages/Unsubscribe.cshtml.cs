using Microsoft.AspNetCore.Mvc;
using PersonalBrand.Core;

namespace PersonalBrand.Web.Pages;

public sealed class UnsubscribeModel(IAudienceService audience) : PublicPage
{
    public bool Done
    {
        get; set;
    }

    public void OnGet()
    {
        Response.Headers.CacheControl = "no-store";
    }

    public async Task<IActionResult> OnPostAsync(string token, CancellationToken ct)
    {
        Done = await audience.UnsubscribeAsync(token, ct);
        if (!Done)
            return NotFound();
        return Page();
    }
}
