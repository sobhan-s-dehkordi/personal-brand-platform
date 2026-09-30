using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PersonalBrand.Core;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed partial class SqlServerTests
{
    [Fact]
    public async Task Bilingual_migration_preserves_existing_ids_and_reservations_and_recovers_unpaired_content()
    {
        await using var db = Db();
        await DevelopmentSeed.RunAsync(db);
        var faCourse = await db.Courses.SingleAsync(x => x.Language == "fa");
        var faWorkshop = await db.Workshops.SingleAsync(x => x.Language == "fa");
        var orphan = new Article { Language = "fa", Slug = "legacy-persian", Title = "Legacy Persian title", Body = "Original Persian content", Status = ContentStatus.Published };
        var lesson = new Lesson { CourseId = faCourse.Id, Slug = "legacy-lesson", Title = "Legacy Persian lesson", ContentBody = "Keep this body", Order = 99, IsPublished = true };
        var contact = new Contact { FullName = "Existing participant" };
        contact.SetEmail("legacy@example.com");
        var reservation = WorkshopReservation.Create(faWorkshop.Id, contact.Id, faWorkshop.Price, faWorkshop.Currency, DateTimeOffset.UtcNow, TimeSpan.FromHours(1));
        db.AddRange(orphan, lesson, contact, reservation);
        await db.SaveChangesAsync();
        var articleIds = await db.Articles.Select(x => x.Id).ToListAsync();
        var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync("20260928132605_DomainAssignedIdentifiers");
        await migrator.MigrateAsync();
        db.ChangeTracker.Clear();
        Assert.False(db.Database.HasPendingModelChanges());
        foreach (var id in articleIds)
            Assert.True(await db.Articles.AnyAsync(x => x.Id == id && x.IsVisible));
        var original = await db.Articles.SingleAsync(x => x.Id == orphan.Id);
        Assert.Equal("Original Persian content", original.Body);
        var counterpart = await db.Articles.SingleAsync(x => x.TranslationGroupId == original.TranslationGroupId && x.Language == "en");
        Assert.False(counterpart.IsVisible);
        Assert.Equal(ContentStatus.Draft, counterpart.Status);
        var originalLesson = await db.Lessons.SingleAsync(x => x.Id == lesson.Id);
        var translatedLesson = await db.Lessons.SingleAsync(x => x.TranslationGroupId == originalLesson.TranslationGroupId && x.Course.Language == "en");
        Assert.False(translatedLesson.IsPublished);
        Assert.Equal("Keep this body", originalLesson.ContentBody);
        Assert.Equal(faWorkshop.Id, (await db.Reservations.SingleAsync(x => x.Id == reservation.Id)).WorkshopId);
        Assert.All(await db.Sessions.ToListAsync(), x => Assert.NotNull(x.TranslationGroupId));
        Assert.Equal(2, await db.Lessons.Where(x => x.Slug == "domain-boundaries").Select(x => x.TranslationGroupId).CountAsync());
        Assert.Single(await db.Lessons.Where(x => x.Slug == "domain-boundaries").Select(x => x.TranslationGroupId).Distinct().ToListAsync());
    }
}
