using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PersonalBrand.Core;

namespace PersonalBrand.Admin.Pages;

[RequestSizeLimit(6 * 1024 * 1024)]
public sealed class MediaModel(IFileStorage storage) : PageModel
{
    [BindProperty]
    public IFormFile? Upload
    {
        get; set;
    }
    public string? PublicPath
    {
        get; set;
    }

    public async Task OnPostAsync(CancellationToken ct)
    {
        if (Upload is null)
        {
            ModelState.AddModelError("", "Choose an image.");
            return;
        }

        try
        {
            await using var stream = Upload.OpenReadStream();
            var file = await storage.StoreAsync(stream, Upload.FileName, true, ct);
            PublicPath = "/media/" + file.Key;
        }
        catch (DomainException e)
        {
            ModelState.AddModelError("", e.Message);
        }
    }
}
