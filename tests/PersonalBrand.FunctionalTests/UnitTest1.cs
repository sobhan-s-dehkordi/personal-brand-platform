using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.FunctionalTests;

public sealed partial class BrowserFlows : IAsyncLifetime
{
    private readonly string connection = BuildConnection();
    private WebApplicationFactory<WebProgram> web = null!;
    private WebApplicationFactory<AdminProgram> admin = null!;
    private static string BuildConnection()
    {
        var b = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("PERSONALBRAND_TEST_SQL") ?? "Server=.;Integrated Security=True;TrustServerCertificate=True");
        b.InitialCatalog = "PersonalBrandFunctional_" + Guid.NewGuid().ToString("N");
        return b.ConnectionString;
    }

    private static byte[] ValidPng()
    {
        using var bitmap = new SkiaSharp.SKBitmap(2, 2);
        bitmap.Erase(SkiaSharp.SKColors.White);
        using var image = SkiaSharp.SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private BrandDbContext Db() => new(new DbContextOptionsBuilder<BrandDbContext>().UseSqlServer(connection).Options);
    private void Configure(IWebHostBuilder builder) => builder.UseEnvironment("Development").ConfigureServices(s =>
    {
        s.RemoveAll<DbContextOptions<BrandDbContext>>();
        s.RemoveAll<BrandDbContext>();
        s.AddDbContext<BrandDbContext>(o => o.UseSqlServer(connection));
    });
    public async Task InitializeAsync()
    {
        await using var db = Db();
        await db.Database.MigrateAsync();
        await DevelopmentSeed.RunAsync(db);
        web = new WebApplicationFactory<WebProgram>().WithWebHostBuilder(Configure);
        admin = new WebApplicationFactory<AdminProgram>().WithWebHostBuilder(Configure);
    }

    public async Task DisposeAsync()
    {
        await web.DisposeAsync();
        await admin.DisposeAsync();
        await using var db = Db();
        await db.Database.EnsureDeletedAsync();
    }

    private static HttpClient Client<T>(WebApplicationFactory<T> f)
        where T : class => f.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, BaseAddress = new Uri("https://localhost") });
    private static async Task<string> Token(HttpClient c, string url)
    {
        var response = await c.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var doc = await new HtmlParser().ParseDocumentAsync(await response.Content.ReadAsStringAsync());
        return doc.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!;
    }

    [Theory]
    [InlineData("/en")]
    [InlineData("/fa")]
    [InlineData("/en/articles/boundaries-before-frameworks")]
    [InlineData("/fa/courses/practical-dotnet")]
    [InlineData("/en/courses/practical-dotnet/lessons/domain-boundaries")]
    [InlineData("/en/workshops/build-with-dotnet")]
    [InlineData("/en/projects/learning-platform")]
    [InlineData("/sitemap.xml")]
    public async Task Public_content_is_accessible(string path)
    {
        using var client = Client(web);
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        if (path == "/fa")
            Assert.Contains("dir=\"rtl\"", html);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Contacts")]
    [InlineData("/Reservations")]
    [InlineData("/Articles/Create")]
    [InlineData("/Settings")]
    [InlineData("/HomeContent")]
    [InlineData("/Administrators")]
    public async Task Admin_requires_authentication(string path)
    {
        using var client = Client(admin);
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Enrollment_requires_csrf_and_creates_contact()
    {
        using var client = Client(web);
        var url = "/en/courses/practical-dotnet/enroll";
        var values = new Dictionary<string, string>
        {
            {
                "Input.FullName",
                "Functional learner"
            },
            {
                "Input.Email",
                "functional@example.com"
            }
        };
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(url, new FormUrlEncodedContent(values))).StatusCode);
        values["__RequestVerificationToken"] = await Token(client, url);
        var response = await client.PostAsync(url, new FormUrlEncodedContent(values));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        await using var db = Db();
        Assert.True(await db.Enrollments.AnyAsync(x => x.Contact.Email == "functional@example.com"));
        Assert.False(await db.Consents.AnyAsync());
    }

    [Fact]
    public async Task Subscription_unsubscribe_is_a_confirmed_post()
    {
        using var client = Client(web);
        var values = new Dictionary<string, string>
        {
            {
                "Input.FullName",
                ""
            },
            {
                "Input.Email",
                "subscribe@example.com"
            },
            {
                "Input.MarketingConsent",
                "true"
            },
            {
                "__RequestVerificationToken",
                await Token(client, "/en")
            }
        };
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/en", new FormUrlEncodedContent(values))).StatusCode);
        await using var db = Db();
        var sub = await db.Subscriptions.AsNoTracking().SingleAsync();
        var url = "/en/unsubscribe/" + sub.UnsubscribeToken;
        var token = await Token(client, url);
        Assert.Equal(SubscriptionStatus.Active, (await db.Subscriptions.AsNoTracking().SingleAsync()).Status);
        var result = await client.PostAsync(url, new FormUrlEncodedContent(new Dictionary<string, string> { { "__RequestVerificationToken", token } }));
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(SubscriptionStatus.Unsubscribed, (await db.Subscriptions.AsNoTracking().SingleAsync()).Status);
    }

    [Fact]
    public async Task Admin_can_create_publish_and_edit_content()
    {
        using var client = Client(admin);
        var password = "T!" + Guid.NewGuid().ToString("N") + "a1";
        using (var scope = admin.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await roles.CreateAsync(new IdentityRole("Administrator"));
            var u = new IdentityUser
            {
                UserName = "editor@example.com",
                Email = "editor@example.com"
            };
            Assert.True((await users.CreateAsync(u, password)).Succeeded);
            await users.AddToRoleAsync(u, "Administrator");
        }

        var login = new Dictionary<string, string>
        {
            {
                "Email",
                "editor@example.com"
            },
            {
                "Password",
                password
            },
            {
                "__RequestVerificationToken",
                await Token(client, "/Account/Login")
            }
        };
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Account/Login", new FormUrlEncodedContent(login))).StatusCode);
        foreach (var kind in new[]
        {
            "Articles",
            "Projects",
            "Courses"
        }

        )
        {
            var html = await client.GetStringAsync("/" + kind + "/Create");
            var values = FormFields(html);
            foreach (var language in new[] { "English", "Persian" })
            {
                values[$"{language}.Fields[Title]"] = language + " published from admin";
                values[$"{language}.Fields[Slug]"] = "admin-created";
                values[$"{language}.Fields[Status]"] = "Published";
                values[$"{language}.Fields[Body]"] = "## Real content\nA working editorial flow.";
                values[$"{language}.IsVisible"] = "true";
                if (kind == "Projects") values[$"{language}.Screenshots"] = "https://example.com/screenshot.png | An example screen";
            }
            if (kind == "Articles")
            {
                values["English.HasRelatedCourses"] = "true";
                values["Persian.HasRelatedCourses"] = "true";
                await using var choices = Db();
                values["English.RelatedCourseIds"] = (await choices.Courses.SingleAsync(x => x.Language == "en")).Id.ToString();
                values["Persian.RelatedCourseIds"] = (await choices.Courses.SingleAsync(x => x.Language == "fa")).Id.ToString();
            }
            var response = await client.PostAsync("/" + kind + "/Create", new FormUrlEncodedContent(values));
            Assert.True(response.StatusCode == HttpStatusCode.Redirect, await response.Content.ReadAsStringAsync());
            using var publicClient = Client(web);
            Assert.Equal(HttpStatusCode.OK, (await publicClient.GetAsync("/en/" + kind.ToLowerInvariant() + "/admin-created")).StatusCode);
            await using var db = Db();
            var id = kind switch
            {
                "Articles" => await db.Articles.Where(x => x.Slug == "admin-created" && x.Language == "en").Select(x => x.Id).SingleAsync(),
                "Projects" => await db.Projects.Where(x => x.Slug == "admin-created" && x.Language == "en").Select(x => x.Id).SingleAsync(),
                _ => await db.Courses.Where(x => x.Slug == "admin-created" && x.Language == "en").Select(x => x.Id).SingleAsync()
            };
            values["__RequestVerificationToken"] = await Token(client, $"/{kind}/Edit/{id}");
            values["English.Fields[Title]"] = "Edited through admin";
            var edited = await client.PostAsync($"/{kind}/Edit/{id}", new FormUrlEncodedContent(values));
            Assert.True(edited.StatusCode == HttpStatusCode.Redirect, await edited.Content.ReadAsStringAsync());
            Assert.Contains("Edited through admin", await publicClient.GetStringAsync("/en/" + kind.ToLowerInvariant() + "/admin-created"));
            if (kind == "Articles")
                Assert.NotEmpty((await db.Articles.Include(x => x.RelatedCourses).SingleAsync(x => x.Id == id)).RelatedCourses);
            if (kind == "Projects")
                Assert.Single((await db.Projects.Include(x => x.Screenshots).SingleAsync(x => x.Id == id)).Screenshots);
        }
    }

    [Fact]
    public async Task Structured_data_and_video_fallback_remain_available()
    {
        using var client = Client(web);
        var doc = await new HtmlParser().ParseDocumentAsync(await client.GetStringAsync("/en/articles/boundaries-before-frameworks"));
        var json = System.Text.Json.JsonDocument.Parse(doc.QuerySelector("script[type='application/ld+json']")!.TextContent);
        Assert.Equal("https://schema.org", json.RootElement.GetProperty("@context").GetString());
        Assert.Equal("Article", json.RootElement.GetProperty("@type").GetString());
        await using var db = Db();
        var lesson = await db.Lessons.SingleAsync(x => x.Slug == "domain-boundaries" && x.Course.Language == "en");
        lesson.YouTubeVideoId = "testvideo01";
        await db.SaveChangesAsync();
        var html = await client.GetStringAsync("/en/courses/practical-dotnet/lessons/domain-boundaries");
        Assert.Contains("https://www.youtube-nocookie.com/embed/testvideo01", html);
        Assert.Contains("Watch on YouTube", html);
        Assert.Contains("strict-origin-when-cross-origin", html);
        Assert.Contains("One simple rule", html);
    }

    [Fact]
    public async Task Workshop_reservation_receipt_and_admin_review()
    {
        using var client = Client(web);
        var url = "/en/workshops/build-with-dotnet/reserve";
        var values = new Dictionary<string, string>
        {
            {
                "Input.FullName",
                "Workshop participant"
            },
            {
                "Input.Email",
                "workshop@example.com"
            },
            {
                "Input.Mobile",
                "+989121234567"
            },
            {
                "__RequestVerificationToken",
                await Token(client, url)
            }
        };
        var result = await client.PostAsync(url, new FormUrlEncodedContent(values));
        Assert.Equal(HttpStatusCode.Redirect, result.StatusCode);
        var reservationUrl = result.Headers.Location!.ToString();
        var token = await Token(client, reservationUrl);
        using var multipart = new MultipartFormDataContent();
        multipart.Add(new StringContent(token), "__RequestVerificationToken");
        multipart.Add(new StringContent("TRACK-1234"), "Tracking");
        multipart.Add(new ByteArrayContent(ValidPng()), "Receipt", "receipt.png");
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync(reservationUrl, multipart)).StatusCode);
        using var adminClient = Client(admin);
        var password = "T!" + Guid.NewGuid().ToString("N") + "a1";
        using (var scope = admin.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await roles.CreateAsync(new IdentityRole("Administrator"));
            var u = new IdentityUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com"
            };
            Assert.True((await users.CreateAsync(u, password)).Succeeded);
            await users.AddToRoleAsync(u, "Administrator");
        }

        var login = new Dictionary<string, string>
        {
            {
                "Email",
                "admin@example.com"
            },
            {
                "Password",
                password
            },
            {
                "__RequestVerificationToken",
                await Token(adminClient, "/Account/Login")
            }
        };
        Assert.Equal(HttpStatusCode.Redirect, (await adminClient.PostAsync("/Account/Login", new FormUrlEncodedContent(login))).StatusCode);
        await using var db = Db();
        var reservation = await db.Reservations.SingleAsync();
        var review = "/Reservations/" + reservation.Id;
        var reviewToken = await Token(adminClient, review);
        var approved = await adminClient.PostAsync(review, new FormUrlEncodedContent(new Dictionary<string, string> { { "__RequestVerificationToken", reviewToken }, { "action", "confirm" }, { "Note", "Verified" } }));
        Assert.Equal(HttpStatusCode.Redirect, approved.StatusCode);
        db.ChangeTracker.Clear();
        Assert.Equal(ReservationStatus.Confirmed, (await db.Reservations.SingleAsync()).Status);
        var receipt = await db.Receipts.SingleAsync();
        var privateUrl = review + "?handler=Receipt&receipt=" + receipt.Id;
        using var anonymousAdmin = Client(admin);
        Assert.Equal(HttpStatusCode.Redirect, (await anonymousAdmin.GetAsync(privateUrl)).StatusCode);
        var image = await adminClient.GetAsync(privateUrl);
        Assert.Equal(HttpStatusCode.OK, image.StatusCode);
        Assert.Equal("image/png", image.Content.Headers.ContentType!.MediaType);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/media/" + receipt.StorageKey)).StatusCode);
        var messages = await db.Notifications.ToListAsync();
        Assert.Equal(3, messages.Count(x => x.Channel == "email" && x.Recipient == "workshop@example.com"));
        Assert.Equal(3, messages.Count(x => x.Channel == "telegram"));
        Assert.Contains(messages, x => x.Subject == "Reservation Confirmed" && x.Body.Contains("Joining details:"));
    }

    [Fact]
    public async Task Administrator_can_create_another_admin_and_duplicate_email_is_rejected()
    {
        using var client = await SignedInAdmin("owner@example.com", "Owner!Password123");
        var fields = new Dictionary<string, string>
        {
            ["Input.Email"] = "second@example.com",
            ["Input.Password"] = "Second!Password123",
            ["Input.ConfirmPassword"] = "Second!Password123"
        };
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/Administrators", new FormUrlEncodedContent(fields))).StatusCode);
        fields["__RequestVerificationToken"] = await Token(client, "/Administrators");
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Administrators", new FormUrlEncodedContent(fields))).StatusCode);
        fields["__RequestVerificationToken"] = await Token(client, "/Administrators");
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync("/Administrators", new FormUrlEncodedContent(fields))).StatusCode);
        using var second = Client(admin);
        Assert.Equal(HttpStatusCode.Redirect, (await second.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string> { ["Email"] = "second@example.com", ["Password"] = "Second!Password123", ["__RequestVerificationToken"] = await Token(second, "/Account/Login") }))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await second.GetAsync("/Administrators")).StatusCode);
        await using var db = Db();
        Assert.Equal(1, await db.Users.CountAsync(x => x.Email == "second@example.com"));
    }

    [Fact]
    public async Task Home_content_is_saved_per_language_and_safe_urls_are_required()
    {
        using var client = await SignedInAdmin("home@example.com", "Home!Password123");
        var document = new HtmlParser().ParseDocument(await client.GetStringAsync("/HomeContent?language=en"));
        var fields = new Dictionary<string, string>();
        foreach (var input in document.QuerySelectorAll("input[name],textarea[name]"))
            fields[input.GetAttribute("name")!] = input.LocalName == "textarea" ? input.TextContent : input.GetAttribute("value") ?? "";
        fields["Values[Home.Heading.en]"] = "A new homepage heading";
        fields["Values[Home.AboutBody.en]"] = "pb-html:<p><strong>A new introduction</strong></p>";
        fields["Values[Brand.GitHubUrl.en]"] = "javascript:alert(1)";
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync("/HomeContent?language=en", new FormUrlEncodedContent(fields))).StatusCode);
        fields["Values[Brand.GitHubUrl.en]"] = "https://github.com/example";
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/HomeContent?language=en", new FormUrlEncodedContent(fields))).StatusCode);
        using var visitor = Client(web);
        var english = await visitor.GetStringAsync("/en");
        Assert.Contains("A new homepage heading", english);
        Assert.Contains("<strong>A new introduction</strong>", english);
        Assert.DoesNotContain("A new homepage heading", await visitor.GetStringAsync("/fa"));
        using var scope = web.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<SiteContent>().EnsureDefaultsAsync();
        Assert.Contains("A new homepage heading", await visitor.GetStringAsync("/en"));
    }

    [Fact]
    public async Task Lesson_editor_accepts_complete_youtube_url()
    {
        using var client = await SignedInAdmin("video@example.com", "Video!Password123");
        await using var db = Db();
        var course = await db.Courses.SingleAsync(x => x.Language == "en");
        var url = $"/Courses/{course.Id}/Lessons";
        var fields = new Dictionary<string, string>
        {
            ["English.Title"] = "YouTube example",
            ["Persian.Title"] = "Persian YouTube example",
            ["English.Body"] = "English lesson body",
            ["Persian.Body"] = "Persian lesson body",
            ["Persian.Slug"] = "youtube-example",
            ["Persian.Order"] = "10",
            ["Persian.Published"] = "false",
            ["English.Slug"] = "youtube-example",
            ["English.Order"] = "10",
            ["English.VideoId"] = "https://www.youtube.com/watch?v=lOKD08QAhyc",
            ["English.Published"] = "true",
            ["__RequestVerificationToken"] = await Token(client, url)
        };
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync(url, new FormUrlEncodedContent(fields))).StatusCode);
        Assert.Equal("lOKD08QAhyc", (await db.Lessons.SingleAsync(x => x.Slug == "youtube-example" && x.Course.Language == "en")).YouTubeVideoId);
        using var visitor = Client(web);
        var html = await visitor.GetStringAsync("/en/courses/practical-dotnet/lessons/youtube-example");
        Assert.Contains("youtube-nocookie.com/embed/lOKD08QAhyc", html);
        Assert.Contains("strict-origin-when-cross-origin", html);
    }

    private async Task<HttpClient> SignedInAdmin(string email, string password)
    {
        var client = Client(admin);
        using var scope = admin.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roles.RoleExistsAsync("Administrator"))
            await roles.CreateAsync(new IdentityRole("Administrator"));
        var user = new IdentityUser
        {
            UserName = email,
            Email = email
        };
        Assert.True((await users.CreateAsync(user, password)).Succeeded);
        await users.AddToRoleAsync(user, "Administrator");
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string> { ["Email"] = email, ["Password"] = password, ["__RequestVerificationToken"] = await Token(client, "/Account/Login") }))).StatusCode);
        return client;
    }

    [Fact]
    public async Task Password_change_requires_current_password_and_updates_login()
    {
        const string original = "Test!Original123", changed = "Test!Changed456";
        using var client = await SignedInAdmin("password@example.com", original);
        const string route = "/Account/ChangePassword";
        var values = new Dictionary<string, string>
        {
            ["CurrentPassword"] = "Wrong!Password123",
            ["NewPassword"] = changed,
            ["ConfirmPassword"] = changed
        };
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(route, new FormUrlEncodedContent(values))).StatusCode);
        values["__RequestVerificationToken"] = await Token(client, route);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync(route, new FormUrlEncodedContent(values))).StatusCode);
        using var scope = admin.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = (await users.FindByEmailAsync("password@example.com"))!;
        Assert.True(await users.CheckPasswordAsync(user, original));
        values["CurrentPassword"] = original;
        values["__RequestVerificationToken"] = await Token(client, route);
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync(route, new FormUrlEncodedContent(values))).StatusCode);
        await scope.ServiceProvider.GetRequiredService<BrandDbContext>().Entry(user).ReloadAsync();
        Assert.True(await users.CheckPasswordAsync(user, changed));
        Assert.False(await users.CheckPasswordAsync(user, original));
        using var fresh = Client(admin);
        Assert.Equal(HttpStatusCode.Redirect, (await fresh.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string> { ["Email"] = "password@example.com", ["Password"] = changed, ["__RequestVerificationToken"] = await Token(fresh, "/Account/Login") }))).StatusCode);
    }

    [Fact]
    public async Task Interface_text_is_static_even_with_legacy_settings()
    {
        using var client = await SignedInAdmin("settings@example.com", "Test!Settings123");
        var settings = await client.GetStringAsync("/Settings");
        var home = await client.GetStringAsync("/HomeContent");
        Assert.DoesNotContain("Values[Empty.", settings);
        Assert.DoesNotContain("Values[Navigation.", home);
        Assert.DoesNotContain("Values[Home.Subscribe]", home);
        await using var db = Db();
        db.Settings.Add(new SiteSetting { Key = "Navigation.articles.en", Value = "Overridden menu" });
        db.Settings.Add(new SiteSetting { Key = "Empty.workshops.en", Value = "Overridden empty message" });
        await db.SaveChangesAsync();
        await db.Workshops.Where(x => x.Language == "en").ExecuteUpdateAsync(s => s.SetProperty(x => x.IsVisible, false));
        using var visitor = Client(web);
        Assert.DoesNotContain("Overridden menu", await visitor.GetStringAsync("/en"));
        Assert.Contains("New work will appear here soon.", await visitor.GetStringAsync("/en/workshops"));
        Assert.DoesNotContain("Overridden empty message", await visitor.GetStringAsync("/en/workshops"));
        Assert.DoesNotContain("New work will appear here soon.", await visitor.GetStringAsync("/en/workshops?q=missing"));
    }
}
