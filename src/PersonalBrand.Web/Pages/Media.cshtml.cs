using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PersonalBrand.Core;

namespace PersonalBrand.Web.Pages;

public sealed class MediaModel(IFileStorage storage) : PageModel
{
    public async Task<IActionResult> OnGetAsync(string key, CancellationToken ct)
    {
        try
        {
            return File(await storage.OpenPublicAsync(key, ct), "image/png");
        }
        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException or DomainException)
        {
            return NotFound();
        }
    }
}
