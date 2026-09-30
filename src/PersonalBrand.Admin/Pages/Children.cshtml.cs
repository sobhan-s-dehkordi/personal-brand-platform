using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Admin.Pages;

public sealed class ChildrenModel(BrandDbContext db) : PageModel
{
    [BindProperty(SupportsGet = true)] public string Kind { get; set; } = "Courses";
    [BindProperty(SupportsGet = true)]
    public Guid Parent
    {
        get; set;
    }
    [BindProperty] public ChildInput English { get; set; } = new();
    [BindProperty] public ChildInput Persian { get; set; } = new();
    public List<ChildRow> Items { get; set; } = [];
    private Guid persianParent;

    private async Task<bool> LoadParents(CancellationToken ct)
    {
        PersonalBrand.Core.Content? parent = Kind == "Courses"
            ? await db.Courses.SingleOrDefaultAsync(x => x.Id == Parent, ct)
            : await db.Workshops.SingleOrDefaultAsync(x => x.Id == Parent, ct);
        if (parent?.TranslationGroupId is not Guid group)
            return false;
        var parents = Kind == "Courses"
            ? await db.Courses.Where(x => x.TranslationGroupId == group).Select(x => new { x.Id, x.Language }).ToListAsync(ct)
            : await db.Workshops.Where(x => x.TranslationGroupId == group).Select(x => new { x.Id, x.Language }).ToListAsync(ct);
        var en = parents.SingleOrDefault(x => x.Language == "en");
        var fa = parents.SingleOrDefault(x => x.Language == "fa");
        if (en is null || fa is null)
            return false;
        Parent = en.Id;
        persianParent = fa.Id;
        Items = Kind == "Courses"
            ? await db.Lessons.Where(x => x.CourseId == Parent).OrderBy(x => x.Order).Select(x => new ChildRow(x.Id, x.Title, x.Order)).ToListAsync(ct)
            : await db.Sessions.Where(x => x.WorkshopId == Parent).OrderBy(x => x.SessionNumber).Select(x => new ChildRow(x.Id, x.Title, x.SessionNumber)).ToListAsync(ct);
        return true;
    }

    public async Task<IActionResult> OnGetAsync(Guid? edit, CancellationToken ct)
    {
        if (!await LoadParents(ct))
            return NotFound();
        if (edit is null)
            return Page();
        if (Kind == "Courses")
        {
            var en = await db.Lessons.SingleOrDefaultAsync(x => x.Id == edit && x.CourseId == Parent, ct);
            if (en is null)
                return NotFound();
            var fa = en.TranslationGroupId is null ? null : await db.Lessons.SingleOrDefaultAsync(x => x.CourseId == persianParent && x.TranslationGroupId == en.TranslationGroupId, ct);
            English = FromLesson(en);
            if (fa is not null)
                Persian = FromLesson(fa);
        }
        else
        {
            var en = await db.Sessions.SingleOrDefaultAsync(x => x.Id == edit && x.WorkshopId == Parent, ct);
            if (en is null)
                return NotFound();
            var fa = en.TranslationGroupId is null ? null : await db.Sessions.SingleOrDefaultAsync(x => x.WorkshopId == persianParent && x.TranslationGroupId == en.TranslationGroupId, ct);
            English = FromSession(en);
            if (fa is not null)
                Persian = FromSession(fa);
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!await LoadParents(ct))
            return NotFound();
        foreach (var (label, input) in new[] { ("English", English), ("Persian", Persian) })
        {
            if (string.IsNullOrWhiteSpace(new SafeMarkdown().PlainText(input.Body)))
                ModelState.AddModelError(label + ".Body", label + " body is required, including when hidden.");
            if (Kind == "Courses" && !System.Text.RegularExpressions.Regex.IsMatch(input.Slug ?? "", "^[a-z0-9]+(?:-[a-z0-9]+)*$"))
                ModelState.AddModelError(label + ".Slug", label + ": a valid lesson slug is required.");
            if (Kind == "Courses" && !string.IsNullOrWhiteSpace(input.VideoId) && YouTubeVideo.Parse(input.VideoId) is null)
                ModelState.AddModelError(label + ".VideoId", label + ": enter a valid YouTube URL.");
            if (Kind == "Workshops" && input.End <= input.Start)
                ModelState.AddModelError(label + ".End", label + ": session end must follow start.");
        }
        if (!ModelState.IsValid)
            return Page();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        if (Kind == "Courses")
        {
            var en = English.Id is null ? new Lesson { CourseId = Parent } : await db.Lessons.SingleOrDefaultAsync(x => x.Id == English.Id && x.CourseId == Parent, ct);
            if (en is null)
                return NotFound();
            var fa = Persian.Id is null ? new Lesson { CourseId = persianParent } : await db.Lessons.SingleOrDefaultAsync(x => x.Id == Persian.Id && x.CourseId == persianParent, ct);
            if (fa is null || Persian.Id is not null && (English.Id is null || en.TranslationGroupId is null || en.TranslationGroupId != fa.TranslationGroupId))
                return BadRequest();
            var group = en.TranslationGroupId ?? Guid.NewGuid();
            en.TranslationGroupId = fa.TranslationGroupId = group;
            ApplyLesson(en, English);
            ApplyLesson(fa, Persian);
            if (English.Id is null)
                db.Lessons.Add(en);
            if (Persian.Id is null)
                db.Lessons.Add(fa);
        }
        else
        {
            var en = English.Id is null ? new WorkshopSession { WorkshopId = Parent } : await db.Sessions.SingleOrDefaultAsync(x => x.Id == English.Id && x.WorkshopId == Parent, ct);
            if (en is null)
                return NotFound();
            var fa = Persian.Id is null ? new WorkshopSession { WorkshopId = persianParent } : await db.Sessions.SingleOrDefaultAsync(x => x.Id == Persian.Id && x.WorkshopId == persianParent, ct);
            if (fa is null || Persian.Id is not null && (English.Id is null || en.TranslationGroupId is null || en.TranslationGroupId != fa.TranslationGroupId))
                return BadRequest();
            var group = en.TranslationGroupId ?? Guid.NewGuid();
            en.TranslationGroupId = fa.TranslationGroupId = group;
            ApplySession(en, English);
            ApplySession(fa, Persian);
            if (English.Id is null)
                db.Sessions.Add(en);
            if (Persian.Id is null)
                db.Sessions.Add(fa);
        }
        try
        {
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException) { ModelState.AddModelError("", "Unable to save both languages. Slug, order, or translation may already exist."); return Page(); }
        TempData["Message"] = "Both languages saved.";
        return RedirectToPage(new
        {
            Kind,
            Parent,
            child = Kind == "Courses" ? "Lessons" : "Sessions"
        });
    }

    private static ChildInput FromLesson(Lesson x) => new() { Id = x.Id, Title = x.Title, Slug = x.Slug, Order = x.Order, Body = x.ContentBody, Summary = x.Summary, KeyPoints = x.KeyPoints, Resources = x.Resources, GitHubUrl = x.GitHubUrl, VideoId = x.YouTubeUrl ?? x.YouTubeVideoId, Duration = x.Duration, Published = x.IsPublished };
    private static ChildInput FromSession(WorkshopSession x) => new() { Id = x.Id, Title = x.Title, Order = x.SessionNumber, Body = x.Description, Start = x.StartAt, End = x.EndAt, Published = x.IsVisible };
    private static void ApplyLesson(Lesson x, ChildInput input)
    {
        x.Title = input.Title;
        x.Order = input.Order;
        x.Slug = input.Slug!;
        x.ContentBody = input.Body;
        x.Summary = input.Summary;
        x.KeyPoints = input.KeyPoints;
        x.Resources = input.Resources;
        x.GitHubUrl = input.GitHubUrl;
        x.YouTubeVideoId = YouTubeVideo.Parse(input.VideoId);
        x.YouTubeUrl = x.YouTubeVideoId is null ? null : "https://www.youtube.com/watch?v=" + x.YouTubeVideoId;
        x.Duration = input.Duration;
        x.IsPublished = input.Published;
    }
    private static void ApplySession(WorkshopSession x, ChildInput input)
    {
        x.Title = input.Title;
        x.SessionNumber = input.Order;
        x.Description = input.Body;
        x.StartAt = input.Start.ToUniversalTime();
        x.EndAt = input.End.ToUniversalTime();
        x.IsVisible = input.Published;
    }
}

public record ChildRow(Guid Id, string Title, int Order);
public sealed class ChildInput
{
    public Guid? Id
    {
        get; set;
    }
    [Required, StringLength(240)] public string Title { get; set; } = "";
    public string? Slug
    {
        get; set;
    }
    [Range(1, 1000)] public int Order { get; set; } = 1;
    public string Body { get; set; } = "";
    public string Summary { get; set; } = "";
    public string KeyPoints { get; set; } = "";
    public string Resources { get; set; } = "";
    [Url]
    public string? GitHubUrl
    {
        get; set;
    }
    [StringLength(1000)]
    public string? VideoId
    {
        get; set;
    }
    [Range(0, 10000)]
    public int Duration
    {
        get; set;
    }
    public bool Published { get; set; } = true;
    public DateTimeOffset Start { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset End { get; set; } = DateTimeOffset.UtcNow.AddHours(2);
}
