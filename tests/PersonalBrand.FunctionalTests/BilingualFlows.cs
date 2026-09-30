using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Core;

namespace PersonalBrand.FunctionalTests;

public sealed partial class BrowserFlows
{
    [Fact]
    public async Task Related_courses_toggle_clears_only_the_disabled_language_and_can_be_enabled_again()
    {
        using var client = await SignedInAdmin("related@example.com", "Test!Related123");
        await using var db = Db();
        var article = await db.Articles.AsNoTracking().SingleAsync(x => x.Language == "en");
        var course = await db.Courses.AsNoTracking().SingleAsync(x => x.Language == "en");
        var persianCourse = await db.Courses.AsNoTracking().SingleAsync(x => x.Language == "fa");
        var route = $"/Articles/Edit/{article.Id}";
        var fields = FormFields(await client.GetStringAsync(route));
        Assert.Equal("true", fields["English.HasRelatedCourses"]);
        fields["English.HasRelatedCourses"] = "false";
        fields["English.RelatedCourseIds"] = course.Id.ToString(); // Stale selections must be ignored.
        fields["Persian.RelatedCourseIds"] = persianCourse.Id.ToString();
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync(route, new FormUrlEncodedContent(fields))).StatusCode);
        Assert.Empty((await db.Articles.Include(x => x.RelatedCourses).AsNoTracking().SingleAsync(x => x.Id == article.Id)).RelatedCourses);
        Assert.Single((await db.Articles.Include(x => x.RelatedCourses).AsNoTracking().SingleAsync(x => x.Language == "fa")).RelatedCourses);
        var document = new HtmlParser().ParseDocument(await client.GetStringAsync(route));
        Assert.True(document.QuerySelector("#English_relatedCourses")!.HasAttribute("hidden"));
        Assert.True(document.QuerySelector("#English_courses")!.HasAttribute("disabled"));
        fields["English.HasRelatedCourses"] = "true";
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync(route, new FormUrlEncodedContent(fields))).StatusCode);
        Assert.Single((await db.Articles.Include(x => x.RelatedCourses).AsNoTracking().SingleAsync(x => x.Id == article.Id)).RelatedCourses);
    }

    [Fact]
    public async Task Admin_pages_render_English_and_bilingual_editors()
    {
        using var client = await SignedInAdmin("preview@example.com", "Test!Preview123");
        await using var db = Db();
        var articleId = await db.Articles.Where(x => x.Language == "en").Select(x => x.Id).SingleAsync();
        var courseId = await db.Courses.Where(x => x.Language == "en").Select(x => x.Id).SingleAsync();
        var pages = new Dictionary<string, string> { ["dashboard"] = "/", ["articles"] = "/Articles", ["article-editor"] = "/Articles/Edit/" + articleId, ["homepage-editor"] = "/HomeContent", ["settings"] = "/Settings", ["lessons"] = $"/Courses/{courseId}/Lessons" };
        foreach (var (name, route) in pages)
        {
            var html = await client.GetStringAsync(route);
            var document = new HtmlParser().ParseDocument(html);
            Assert.Equal("en", document.DocumentElement.GetAttribute("lang"));
            Assert.Equal("ltr", document.DocumentElement.GetAttribute("dir"));
            Assert.DoesNotMatch("[\\u0600-\\u06ff]", document.QuerySelector(".sidebar")!.TextContent);
            if (name == "article-editor")
            {
                Assert.Equal("rtl", document.QuerySelector("textarea[name='Persian.Fields[Body]']")!.GetAttribute("dir"));
                Assert.Equal("ltr", document.QuerySelector("textarea[name='English.Fields[Body]']")!.GetAttribute("dir"));
            }
            if (Environment.GetEnvironmentVariable("PERSONALBRAND_UI_REVIEW") is string output)
            {
                Directory.CreateDirectory(output);
                await File.WriteAllTextAsync(Path.Combine(output, name + ".html"), html);
            }
        }
    }

    private static Dictionary<string, string> FormFields(string html)
    {
        var doc = new HtmlParser().ParseDocument(html);
        var fields = new Dictionary<string, string>();
        foreach (var input in doc.QuerySelectorAll("form input[name],form textarea[name],form select[name]"))
        {
            var name = input.GetAttribute("name")!;
            if (input.GetAttribute("type") == "checkbox" && !input.HasAttribute("checked"))
                continue;
            if (input.GetAttribute("type") == "hidden" && fields.ContainsKey(name))
                continue;
            if (input.LocalName == "select" && input.HasAttribute("multiple"))
                continue;
            fields[name] = input.LocalName == "textarea" ? input.TextContent : input.LocalName == "select"
                ? (input.QuerySelector("option[selected]") ?? input.QuerySelector("option"))?.GetAttribute("value") ?? ""
                : input.GetAttribute("value") ?? "";
        }
        return fields;
    }

    [Fact]
    public async Task Both_languages_are_required_and_failed_save_is_atomic()
    {
        using var client = await SignedInAdmin("bilingual@example.com", "Test!Bilingual123");
        await using var db = Db();
        var original = await db.Articles.AsNoTracking().SingleAsync(x => x.Language == "en");
        var persian = await db.Articles.AsNoTracking().SingleAsync(x => x.Language == "fa");
        var route = $"/Articles/Edit/{original.Id}";
        var fields = FormFields(await client.GetStringAsync(route));
        Assert.Contains("English.Fields[Title]", fields.Keys);
        Assert.Contains("Persian.Fields[Title]", fields.Keys);
        fields["English.Fields[Title]"] = "Must not be saved";
        fields["Persian.Fields[Body]"] = "pb-html:<p><br></p>";
        fields["Persian.IsVisible"] = "false";
        var result = await client.PostAsync(route, new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Contains("Persian body is required", await result.Content.ReadAsStringAsync());
        Assert.Equal(original.Title, (await db.Articles.AsNoTracking().SingleAsync(x => x.Id == original.Id)).Title);
        Assert.Equal(persian.Body, (await db.Articles.AsNoTracking().SingleAsync(x => x.Id == persian.Id)).Body);

        fields["Persian.Fields[Body]"] = "Updated Persian body";
        fields["English.Fields[Title]"] = "Updated English title";
        fields["English.IsVisible"] = "false";
        fields["Persian.IsVisible"] = "true";
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync(route, new FormUrlEncodedContent(fields))).StatusCode);
        Assert.Equal(2, await db.Articles.CountAsync());
        Assert.Equal(original.TranslationGroupId, (await db.Articles.AsNoTracking().SingleAsync(x => x.Id == persian.Id)).TranslationGroupId);
        using var visitor = Client(web);
        Assert.Equal(HttpStatusCode.NotFound, (await visitor.GetAsync("/en/articles/" + original.Slug)).StatusCode);
        var faHtml = await visitor.GetStringAsync("/fa/articles/" + persian.Slug);
        Assert.DoesNotContain("hreflang=\"en\"", faHtml);
        Assert.DoesNotContain("Updated English title", await visitor.GetStringAsync("/en/articles"));
        Assert.DoesNotContain("Updated English title", await visitor.GetStringAsync("/en"));
        Assert.DoesNotContain("/en/articles/" + original.Slug, await visitor.GetStringAsync("/sitemap.xml"));
        var list = new HtmlParser().ParseDocument(await client.GetStringAsync("/Articles"));
        Assert.Single(list.QuerySelectorAll("tbody tr"));
        Assert.Contains("Updated English title", list.Body!.TextContent);
        Assert.DoesNotContain(persian.Title, list.QuerySelector("tbody")!.TextContent);
    }

    [Fact]
    public async Task Workshop_languages_keep_capacity_and_reservations_independent()
    {
        using var client = await SignedInAdmin("workshop-editor@example.com", "Test!Workshops123");
        await using var db = Db();
        var en = await db.Workshops.SingleAsync(x => x.Language == "en");
        var fa = await db.Workshops.SingleAsync(x => x.Language == "fa");
        var route = $"/Workshops/Edit/{en.Id}";
        var fields = FormFields(await client.GetStringAsync(route));
        fields["English.Fields[Capacity]"] = "3";
        fields["Persian.Fields[Capacity]"] = "9";
        fields["English.IsVisible"] = "false";
        fields["Persian.IsVisible"] = "true";
        var saved = await client.PostAsync(route, new FormUrlEncodedContent(fields));
        Assert.True(saved.StatusCode == HttpStatusCode.Redirect, await saved.Content.ReadAsStringAsync());
        await db.Entry(en).ReloadAsync();
        await db.Entry(fa).ReloadAsync();
        Assert.Equal(3, en.Capacity);
        Assert.Equal(9, fa.Capacity);
        using var visitor = Client(web);
        Assert.Equal(HttpStatusCode.NotFound, (await visitor.GetAsync("/en/workshops/" + en.Slug + "/reserve")).StatusCode);
        var reserveRoute = "/fa/workshops/" + fa.Slug + "/reserve";
        var result = await visitor.PostAsync(reserveRoute, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = await Token(visitor, reserveRoute),
            ["Input.FullName"] = "Persian participant",
            ["Input.Email"] = "independent@example.com",
            ["Input.Mobile"] = "+989121234567"
        }));
        Assert.Equal(HttpStatusCode.Redirect, result.StatusCode);
        Assert.Equal(fa.Id, (await db.Reservations.SingleAsync()).WorkshopId);
        Assert.False(await db.Reservations.AnyAsync(x => x.WorkshopId == en.Id));
        // Hiding a workshop preserves access to the participant's existing private reservation.
        fields["Persian.IsVisible"] = "false";
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync(route, new FormUrlEncodedContent(fields))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await visitor.GetAsync(result.Headers.Location)).StatusCode);
    }

    [Theory]
    [InlineData("Courses", "Lessons")]
    [InlineData("Workshops", "Sessions")]
    public async Task Child_editor_updates_both_existing_languages_and_preserves_visibility(string kind, string child)
    {
        using var client = await SignedInAdmin("children@example.com", "Test!Children123");
        await using var db = Db();
        var parent = kind == "Courses" ? await db.Courses.Where(x => x.Language == "en").Select(x => x.Id).SingleAsync()
            : await db.Workshops.Where(x => x.Language == "en").Select(x => x.Id).SingleAsync();
        var id = kind == "Courses" ? await db.Lessons.Where(x => x.CourseId == parent && x.Order == 1).Select(x => x.Id).SingleAsync()
            : await db.Sessions.Where(x => x.WorkshopId == parent && x.SessionNumber == 1).Select(x => x.Id).SingleAsync();
        var route = $"/{kind}/{parent}/{child}?edit={id}";
        var fields = FormFields(await client.GetStringAsync(route));
        Assert.False(string.IsNullOrWhiteSpace(fields["English.Id"]));
        Assert.False(string.IsNullOrWhiteSpace(fields["Persian.Id"]));
        fields["English.Title"] = "Updated English child";
        fields["Persian.Title"] = "Updated Persian child";
        fields["English.Body"] = "English child body";
        fields["Persian.Body"] = "Persian child body";
        fields["English.Published"] = "false";
        fields["Persian.Published"] = "true";
        var result = await client.PostAsync(route, new FormUrlEncodedContent(fields));
        Assert.True(result.StatusCode == HttpStatusCode.Redirect, await result.Content.ReadAsStringAsync());
        using var visitor = Client(web);
        if (kind == "Courses")
        {
            Assert.Equal(2, await db.Lessons.CountAsync(x => x.Title.StartsWith("Updated")));
            Assert.Equal(HttpStatusCode.NotFound, (await visitor.GetAsync("/en/courses/practical-dotnet/lessons/domain-boundaries")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await visitor.GetAsync("/fa/courses/practical-dotnet/lessons/domain-boundaries")).StatusCode);
        }
        else
        {
            Assert.Equal(2, await db.Sessions.CountAsync(x => x.Title.StartsWith("Updated")));
            Assert.DoesNotContain("Updated English child", await visitor.GetStringAsync("/en/workshops/build-with-dotnet"));
            Assert.Contains("Updated Persian child", await visitor.GetStringAsync("/fa/workshops/build-with-dotnet"));
        }
    }
}
