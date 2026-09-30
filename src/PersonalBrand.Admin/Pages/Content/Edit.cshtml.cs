using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Admin.Pages.Content;

public sealed class EditModel(BrandDbContext db, TimeProvider clock, ILogger<EditModel> logger) : PageModel
{
    private static readonly string[] LeadingFields = ["Title", "Language", "Slug", "Summary", "Body", "CoverImage", "Status", "IsFeatured"];
    [BindProperty(SupportsGet = true)]
    public string Kind { get; set; } = "Articles";

    [BindProperty]
    public LanguageInput English { get; set; } = new();
    [BindProperty]
    public LanguageInput Persian { get; set; } = new();
    private LanguageInput current = new();
    public Dictionary<string, string?> Fields
    {
        get => current.Fields; set => current.Fields = value;
    }

    public string? Tags
    {
        get => current.Tags; set => current.Tags = value;
    }
    public string? Screenshots
    {
        get => current.Screenshots; set => current.Screenshots = value;
    }
    public Guid[] RelatedCourseIds
    {
        get => current.RelatedCourseIds; set => current.RelatedCourseIds = value;
    }
    public Dictionary<string, PersonalBrand.Core.Content> Versions { get; private set; } = [];
    public List<CourseChoice> CourseOptions { get; set; } = [];
    public PersonalBrand.Core.Content Item { get; set; } = null!;
    public PropertyInfo[] Properties => Editable(Item);

    public static string Label(string property) => System.Text.RegularExpressions.Regex.Replace(property, "(?<=[a-z])([A-Z])", " $1");
    private static PropertyInfo[] Editable(PersonalBrand.Core.Content item) => item.GetType().GetProperties().Where(p => p.CanWrite && p.Name is not ("Id" or "UpdatedAt" or "Language" or "TranslationGroupId" or "IsVisible") && (p.PropertyType == typeof(string) || p.PropertyType.IsEnum || p.PropertyType == typeof(int) || p.PropertyType == typeof(decimal) || p.PropertyType == typeof(bool) || p.PropertyType == typeof(DateTimeOffset) || p.PropertyType == typeof(DateTimeOffset?) || p.PropertyType == typeof(Guid?))).OrderBy(p => Array.IndexOf(LeadingFields, p.Name) is var i && i >= 0 ? i : 100).ToArray();
    private async Task LoadOptions(CancellationToken ct)
    {
        CourseOptions = await db.Courses.AsNoTracking().OrderBy(x => x.Title).Select(x => new CourseChoice(x.Id, x.Title + " (" + x.Language + ")")).Take(500).ToListAsync(ct);
    }

    public async Task<IActionResult> OnGetAsync(Guid? id, CancellationToken ct)
    {
        if (!await LoadPair(id, ct))
            return NotFound();
        await LoadOptions(ct);
        foreach (var language in new[] { "en", "fa" })
        {
            Item = Versions[language];
            current = language == "en" ? English : Persian;
            current.IsVisible = Item.IsVisible;
            Fields = Properties.ToDictionary(p => p.Name, p => Convert.ToString(p.GetValue(Item), CultureInfo.InvariantCulture));
            if (Item is Article article)
            {
                Tags = string.Join(", ", article.Tags.Select(x => x.Name));
                RelatedCourseIds = article.RelatedCourses.Select(x => x.Id).ToArray();
                current.HasRelatedCourses = RelatedCourseIds.Length > 0;
            }

            if (Item is Project project)
            {
                Tags = string.Join(", ", project.Technologies.Select(x => x.Name));
                Screenshots = string.Join("\n", project.Screenshots.Select(x => x.Path + " | " + x.Alt));
            }

        }
        Item = Versions["en"];
        return Page();
    }

    private async Task<bool> LoadPair(Guid? id, CancellationToken ct)
    {
        var source = await Load(id, ct);
        if (id is not null && source is null)
            return false;
        var group = source?.TranslationGroupId ?? Guid.NewGuid();
        PersonalBrand.Core.Content? other = null;
        if (source?.TranslationGroupId is Guid existing)
        {
            Guid? otherId = Kind switch
            {
                "Articles" => await db.Articles.Where(x => x.TranslationGroupId == existing && x.Language != source.Language).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(ct),
                "Courses" => await db.Courses.Where(x => x.TranslationGroupId == existing && x.Language != source.Language).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(ct),
                "Projects" => await db.Projects.Where(x => x.TranslationGroupId == existing && x.Language != source.Language).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(ct),
                _ => await db.Workshops.Where(x => x.TranslationGroupId == existing && x.Language != source.Language).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(ct)
            };
            other = await Load(otherId, ct);
        }
        foreach (var language in new[] { "en", "fa" })
        {
            var version = source?.Language == language ? source : other?.Language == language ? other : New();
            version.Language = language;
            version.TranslationGroupId = group;
            Versions[language] = version;
        }
        Item = Versions["en"];
        return true;
    }

    private PersonalBrand.Core.Content New() => Kind switch
    {
        "Articles" => new Article(),
        "Courses" => new Course(),
        "Projects" => new Project(),
        _ => new Workshop
        {
            RegistrationOpensAt = clock.GetUtcNow(),
            RegistrationClosesAt = clock.GetUtcNow().AddDays(7),
            StartDate = clock.GetUtcNow().AddDays(8),
            EndDate = clock.GetUtcNow().AddDays(9)
        }
    };
    private async Task<PersonalBrand.Core.Content?> Load(Guid? id, CancellationToken ct) => id is null ? null : Kind switch
    {
        "Articles" => await db.Articles.Include(x => x.Tags).Include(x => x.RelatedCourses).SingleOrDefaultAsync(x => x.Id == id, ct),
        "Courses" => await db.Courses.SingleOrDefaultAsync(x => x.Id == id, ct),
        "Projects" => await db.Projects.Include(x => x.Technologies).Include(x => x.Screenshots).SingleOrDefaultAsync(x => x.Id == id, ct),
        _ => await db.Workshops.FromSqlInterpolated($"SELECT * FROM Workshops WITH (UPDLOCK,HOLDLOCK) WHERE Id={id.Value}").SingleOrDefaultAsync(ct)
    };
    public async Task<IActionResult> OnPostAsync(Guid? id, CancellationToken ct)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        if (!await LoadPair(id, ct))
            return NotFound();
        await LoadOptions(ct);
        foreach (var language in new[] { "en", "fa" })
        {
            Item = Versions[language];
            current = language == "en" ? English : Persian;
            Item.IsVisible = current.IsVisible;
            foreach (var property in Properties)
            {
                Fields.TryGetValue(property.Name, out var value);
                try
                {
                    property.SetValue(Item, ConvertValue(property, value));
                }
                catch (Exception e) when (e is FormatException or ArgumentException or OverflowException)
                {
                    ModelState.AddModelError((language == "en" ? "English" : "Persian") + ".Fields[" + property.Name + "]", "Invalid " + Label(property.Name));
                }
            }

            if (string.IsNullOrWhiteSpace(Item.Slug))
                Item.Slug = "content-" + Item.Id.ToString("N")[..12];
            if (string.IsNullOrWhiteSpace(new SafeMarkdown().PlainText(Item.Body)))
                ModelState.AddModelError("", (language == "en" ? "English" : "Persian") + " body is required, including when hidden.");
            var errors = new List<ValidationResult>();
            Validator.TryValidateObject(Item, new ValidationContext(Item), errors, true);
            foreach (var error in errors)
                ModelState.AddModelError("", (language == "en" ? "English: " : "Persian: ") + error.ErrorMessage!);
            foreach (var property in Properties.Where(p => p.PropertyType == typeof(string) && (p.Name.EndsWith("Url") || p.Name == "CoverImage")))
            {
                var value = property.GetValue(Item) as string;
                if (!string.IsNullOrEmpty(value) && !SafePublicUrl(value))
                    ModelState.AddModelError("", Label(property.Name) + " must be HTTPS or an uploaded /media/ path.");
            }

            if (Item is Workshop workshop)
            {
                if (workshop.RegistrationClosesAt <= workshop.RegistrationOpensAt || workshop.StartDate < workshop.RegistrationClosesAt || workshop.EndDate < workshop.StartDate)
                    ModelState.AddModelError("", "Check workshop registration and session dates.");
                if (!TimeZoneInfo.TryFindSystemTimeZoneById(workshop.TimeZoneId, out _))
                    ModelState.AddModelError("", "Choose a valid timezone, for example Asia/Tehran.");
                var occupied = await ReservationService.Active(db.Reservations.Where(x => x.WorkshopId == Item.Id), clock.GetUtcNow()).CountAsync(ct);
                if (workshop.Capacity < occupied)
                    ModelState.AddModelError("", "Capacity cannot be lower than occupied seats.");
            }

            var selectedCourses = new List<Course>();
            if (Item is Article)
            {
                var ids = current.HasRelatedCourses ? RelatedCourseIds.Distinct().ToArray() : [];
                if (ids.Length > 20)
                    ModelState.AddModelError("", "Select at most 20 related courses.");
                else
                {
                    selectedCourses = await db.Courses.Where(x => ids.Contains(x.Id) && x.Language == language).ToListAsync(ct);
                    if (selectedCourses.Count != ids.Length)
                        ModelState.AddModelError("", "A selected course no longer exists.");
                }
            }

            var screenshots = new List<ProjectImage>();
            if (Item is Project)
            {
                foreach (var line in (Screenshots ?? "").Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = line.Split('|', 2, StringSplitOptions.TrimEntries);
                    if (parts.Length != 2 || !SafePublicUrl(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                        ModelState.AddModelError("", "Each screenshot needs a safe image URL and alternative text, separated by |.");
                    else
                        screenshots.Add(new ProjectImage { ProjectId = Item.Id, Path = parts[0], Alt = parts[1] });
                }

                if (screenshots.Count > 20)
                    ModelState.AddModelError("", "Use at most 20 screenshots.");
            }

            if (!ModelState.IsValid)
                continue;
            if (Item is Article article)
            {
                article.Tags.Clear();
                foreach (var name in SplitTags())
                {
                    var tag = db.Set<Tag>().Local.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))
                        ?? await db.Set<Tag>().FirstOrDefaultAsync(x => x.Name == name, ct);
                    if (tag is null)
                    {
                        tag = new Tag { Name = name };
                        db.Add(tag);
                    }
                    article.Tags.Add(tag);
                }
                article.RelatedCourses.Clear();
                article.RelatedCourses.AddRange(selectedCourses);
            }

            if (Item is Project project)
            {
                project.Technologies.Clear();
                foreach (var name in SplitTags())
                {
                    var technology = db.Set<Technology>().Local.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))
                        ?? await db.Set<Technology>().FirstOrDefaultAsync(x => x.Name == name, ct);
                    if (technology is null)
                    {
                        technology = new Technology { Name = name };
                        db.Add(technology);
                    }
                    project.Technologies.Add(technology);
                }
                db.RemoveRange(project.Screenshots);
                project.Screenshots.Clear();
                project.Screenshots.AddRange(screenshots);
            }

            Item.UpdatedAt = clock.GetUtcNow();
            if (Item.Status == ContentStatus.Published)
                Item.PublishedAt ??= clock.GetUtcNow();
            if (db.Entry(Item).State == EntityState.Detached)
                db.Add(Item);
            if (Item is Workshop { WorkshopStatus: WorkshopStatus.Cancelled })
            {
                var reservations = await ReservationService.Active(db.Reservations.Include(x => x.Contact).Where(x => x.WorkshopId == Item.Id), clock.GetUtcNow()).ToListAsync(ct);
                foreach (var reservation in reservations)
                {
                    reservation.Cancel(clock.GetUtcNow(), adminId, "Workshop cancelled");
                    db.Notifications.Add(new Notification { Recipient = reservation.Contact.Email, Subject = "Workshop cancelled", Body = $"Workshop {Item.Title} has been cancelled. Please contact the organizer regarding any transferred payment.", CreatedAt = clock.GetUtcNow() });
                }
            }

        }
        Item = Versions["en"];
        if (!ModelState.IsValid)
            return Page();
        try
        {
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Unable to save. A language/slug combination may already exist.");
            return Page();
        }

        logger.LogInformation("Admin {AdminId} saved {Kind} {Id}", adminId, Kind, Item.Id);
        TempData["Message"] = "Content saved.";
        return Redirect("/" + Kind);
    }

    private static object? ConvertValue(PropertyInfo property, string? value)
    {
        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        if (string.IsNullOrWhiteSpace(value) && Nullable.GetUnderlyingType(property.PropertyType) is not null)
            return null;
        if (type == typeof(string))
            return string.IsNullOrWhiteSpace(value) && (property.Name.EndsWith("Url") || property.Name is "CoverImage" or "SeoTitle" or "SeoDescription") ? null : value ?? "";
        if (type.IsEnum)
        {
            var parsed = Enum.Parse(type, value ?? "");
            return Enum.IsDefined(type, parsed) ? parsed : throw new ArgumentException("Unknown status.");
        }

        if (type == typeof(Guid))
            return Guid.Parse(value!);
        if (type == typeof(DateTimeOffset))
            return DateTimeOffset.Parse(value!, CultureInfo.InvariantCulture).ToUniversalTime();
        return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
    }

    private static bool SafePublicUrl(string value) => System.Text.RegularExpressions.Regex.IsMatch(value, "^/media/[a-f0-9]{32}\\.png$") || Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == "https" && string.IsNullOrEmpty(uri.UserInfo);
    private IEnumerable<string> SplitTags() => (Tags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(x => x[..Math.Min(80, x.Length)]).Distinct().Take(20);
}

public sealed class LanguageInput
{
    public bool HasRelatedCourses { get; set; }
    public Dictionary<string, string?> Fields { get; set; } = [];
    public bool IsVisible { get; set; } = true;
    [StringLength(2000)]
    public string? Tags
    {
        get; set;
    }
    [StringLength(10000)]
    public string? Screenshots
    {
        get; set;
    }
    public Guid[] RelatedCourseIds { get; set; } = [];
}

