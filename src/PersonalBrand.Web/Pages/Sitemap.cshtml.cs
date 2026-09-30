using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Web.Pages;

public sealed class SitemapModel(BrandDbContext db, IOptions<SiteOptions> site) : PageModel
{
    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var paths = new List<string>();
        foreach (var culture in new[]
        {
            "en",
            "fa"
        }

        )
        {
            paths.Add("/" + culture);
            foreach (var page in new[]
            {
                "articles",
                "courses",
                "projects",
                "workshops",
                "about",
                "resume",
                "now",
                "contact",
                "privacy"
            }

            )
                paths.Add($"/{culture}/{page}");
        }

        paths.AddRange(await Read(db.Articles, "articles", ct));
        paths.AddRange(await Read(db.Courses, "courses", ct));
        paths.AddRange(await Read(db.Projects, "projects", ct));
        paths.AddRange(await Read(db.Workshops, "workshops", ct));
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        return Content(new XDocument(new XElement(ns + "urlset", paths.Select(p => new XElement(ns + "url", new XElement(ns + "loc", site.Value.BaseUrl.TrimEnd('/') + p))))).ToString(), "application/xml");
    }

    private Task<List<string>> Read<T>(IQueryable<T> q, string section, CancellationToken ct)
        where T : PersonalBrand.Core.Content => q.AsNoTracking().Where(x => x.IsVisible && x.Status == ContentStatus.Published).OrderBy(x => x.Id).Take(10000).Select(x => "/" + x.Language + "/" + section + "/" + x.Slug).ToListAsync(ct);
}
