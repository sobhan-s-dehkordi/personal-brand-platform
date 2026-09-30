using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Web.Pages;

public sealed class InfoModel(BrandDbContext db) : PublicPage
{
    public string Name { get; set; } = "";
    public string Body { get; set; } = "";

    public async Task OnGetAsync(string name, CancellationToken ct)
    {
        Name = name;
        Body = await db.Settings.Where(x => x.Key == "Page." + name + "." + Culture).Select(x => x.Value).FirstOrDefaultAsync(ct) ?? "";
    }
}
