using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed partial class SqlServerTests : IAsyncLifetime
{
    private readonly string connection = BuildConnection();
    private readonly string root = Path.Combine(Path.GetTempPath(), "PersonalBrandTests", Guid.NewGuid().ToString("N"));
    private static string BuildConnection()
    {
        var b = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("PERSONALBRAND_TEST_SQL") ?? "Server=.;Integrated Security=True;TrustServerCertificate=True");
        b.InitialCatalog = "PersonalBrandTest_" + Guid.NewGuid().ToString("N");
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
    public async Task InitializeAsync()
    {
        await using var db = Db();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var db = Db();
        await db.Database.EnsureDeletedAsync();
    }

    private AudienceService Audience(BrandDbContext db) => new(db, TimeProvider.System, Options.Create(new SiteOptions()));
    private ReservationService Service(BrandDbContext db) => new(db, Audience(db), TimeProvider.System, Options.Create(new WorkshopReservationOptions()), Options.Create(new SiteOptions()), new LocalFileStorage(Options.Create(new StorageOptions { Root = root })));
    private static ParticipantInput Person(string email, bool consent = false) => new()
    {
        FullName = "Test participant",
        Email = email,
        Mobile = "+989121234567",
        MarketingConsent = consent
    };
    private async Task<Workshop> Workshop(int capacity = 7)
    {
        await using var db = Db();
        var now = DateTimeOffset.UtcNow;
        var w = new Workshop
        {
            Slug = Guid.NewGuid().ToString("N"),
            Title = "Concurrency workshop",
            Status = ContentStatus.Published,
            WorkshopStatus = WorkshopStatus.Open,
            Capacity = capacity,
            Price = 100,
            RegistrationOpensAt = now.AddDays(-1),
            RegistrationClosesAt = now.AddDays(1),
            StartDate = now.AddDays(2),
            EndDate = now.AddDays(3)
        };
        db.Workshops.Add(w);
        await db.SaveChangesAsync();
        return w;
    }

    [Fact]
    public async Task Twenty_concurrent_requests_never_exceed_seven_seats()
    {
        var w = await Workshop();
        var results = await Task.WhenAll(Enumerable.Range(0, 20).Select(async i =>
        {
            await using var db = Db();
            try
            {
                await Service(db).ReserveAsync(w.Id, Person($"seat{i}@example.com"), default);
                return true;
            }
            catch (DomainException)
            {
                return false;
            }
        }));
        Assert.Equal(7, results.Count(x => x));
        await using var verify = Db();
        Assert.Equal(7, await ReservationService.Active(verify.Reservations.Where(x => x.WorkshopId == w.Id), DateTimeOffset.UtcNow).CountAsync());
    }

    [Fact]
    public async Task Enrollment_deduplicates_and_consent_is_separate()
    {
        Guid course;
        await using (var db = Db())
        {
            var c = new Course
            {
                Title = "Free",
                Slug = "free",
                Status = ContentStatus.Published
            };
            db.Courses.Add(c);
            await db.SaveChangesAsync();
            course = c.Id;
        }

        await using (var db = Db())
            await Audience(db).EnrollAsync(course, Person("learner@example.com"), default);
        await using (var db = Db())
            await Audience(db).EnrollAsync(course, Person("LEARNER@example.com"), default);
        await using (var db = Db())
        {
            Assert.Equal(1, await db.Enrollments.CountAsync());
            Assert.Empty(await db.Consents.ToListAsync());
            Assert.Empty(await db.Subscriptions.ToListAsync());
        }

        await using (var db = Db())
            await Audience(db).SubscribeAsync(Person("learner@example.com", true), default);
        string token;
        await using (var db = Db())
        {
            token = (await db.Subscriptions.SingleAsync()).UnsubscribeToken;
            Assert.Equal(1, await db.Contacts.CountAsync());
        }

        await using (var db = Db())
            Assert.True(await Audience(db).UnsubscribeAsync(token, default));
        await using (var db = Db())
        {
            Assert.Equal(SubscriptionStatus.Unsubscribed, (await db.Subscriptions.SingleAsync()).Status);
            Assert.All(await db.Consents.ToListAsync(), x => Assert.NotNull(x.RevokedAt));
            Assert.Equal(1, await db.Enrollments.CountAsync());
        }
    }

    [Fact]
    public async Task Receipt_review_is_transactional_and_private()
    {
        var w = await Workshop(1);
        WorkshopReservation r;
        await using (var db = Db())
            r = await Service(db).ReserveAsync(w.Id, Person("receipt@example.com"), default);
        byte[] png = ValidPng();
        await using (var db = Db())
            await Service(db).SubmitAsync(r.AccessToken, "TRACK-123", new MemoryStream(png), "receipt.png", default);
        await using (var db = Db())
        {
            var receipt = await db.Receipts.SingleAsync();
            Assert.DoesNotContain("wwwroot", receipt.StorageKey);
            Assert.Equal(ReservationStatus.PaymentSubmitted, (await db.Reservations.SingleAsync()).Status);
        }

        await using (var db = Db())
            await Service(db).ReviewAsync(r.Id, "confirm", "admin", "Verified", default);
        await using (var db = Db())
        {
            Assert.Equal(ReservationStatus.Confirmed, (await db.Reservations.SingleAsync()).Status);
            Assert.Equal(3, await db.Set<ReservationHistory>().CountAsync());
            Assert.True(await db.Notifications.AnyAsync());
        }
    }

    [Fact]
    public async Task Expired_hold_releases_capacity_without_cleanup()
    {
        var w = await Workshop(1);
        string expiredToken;
        await using (var db = Db())
        {
            var contact = new Contact
            {
                FullName = "Expired"
            };
            contact.SetEmail("expired@example.com");
            db.Contacts.Add(contact);
            var expired = WorkshopReservation.Create(w.Id, contact.Id, 100, "IRR", DateTimeOffset.UtcNow.AddHours(-3), TimeSpan.FromHours(2));
            expiredToken = expired.AccessToken;
            db.Reservations.Add(expired);
            await db.SaveChangesAsync();
        }

        await using (var db = Db())
            await Service(db).ReserveAsync(w.Id, Person("newseat@example.com"), default);
        await using (var db = Db())
            Assert.Equal(1, await ReservationService.Active(db.Reservations.Where(x => x.WorkshopId == w.Id), DateTimeOffset.UtcNow).CountAsync());
        await using (var db = Db())
            await Assert.ThrowsAsync<DomainException>(() => Service(db).SubmitAsync(expiredToken, "LATE123", new MemoryStream(ValidPng()), "receipt.png", default));
        await using (var db = Db())
            Assert.False(await db.Receipts.AnyAsync());
    }

    [Fact]
    public async Task Unique_contact_constraint_is_enforced()
    {
        await using var db = Db();
        var a = new Contact();
        a.SetEmail("duplicate@example.com");
        var b = new Contact();
        b.SetEmail("DUPLICATE@example.com");
        db.Contacts.AddRange(a, b);
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Upload_rejects_mismatched_type_and_traversal()
    {
        var storage = new LocalFileStorage(Options.Create(new StorageOptions { Root = root }));
        await Assert.ThrowsAsync<DomainException>(() => storage.StoreAsync(new MemoryStream("<script>"u8.ToArray()), "photo.png", false, default));
        await Assert.ThrowsAsync<DomainException>(() => storage.OpenPrivateAsync("../../secret", default));
    }

    [Fact]
    public void Markdown_strips_active_content()
    {
        var html = new SafeMarkdown().Render("[unsafe](javascript:alert(1))\n<script>alert(1)</script>");
        Assert.DoesNotContain("href=\"javascript", html);
        Assert.DoesNotContain("<script>", html);
    }
}

