using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalBrand.Core;

namespace PersonalBrand.Infrastructure.Configurations;

internal static class ContentMapping
{
    public static void Configure<T>(EntityTypeBuilder<T> b)
        where T : Content
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.Language, x.Slug }).IsUnique();
        b.HasIndex(x => new { x.Status, x.PublishedAt });
        b.HasIndex(x => x.TranslationGroupId);
        b.HasIndex(x => new { x.TranslationGroupId, x.Language }).IsUnique().HasFilter("[TranslationGroupId] IS NOT NULL");
        b.Property(x => x.Language).HasMaxLength(2);
        b.Property(x => x.Slug).HasMaxLength(160);
        b.Property(x => x.Title).HasMaxLength(240);
        b.Property(x => x.Summary).HasMaxLength(1000);
    }
}

public sealed class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> b)
    {
        ContentMapping.Configure(b);
        b.HasMany(x => x.Tags).WithMany();
        b.HasMany(x => x.RelatedCourses).WithMany();
    }
}

public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> b)
    {
        ContentMapping.Configure(b);
        b.HasMany(x => x.Lessons).WithOne(x => x.Course).HasForeignKey(x => x.CourseId);
    }
}

public sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> b)
    {
        b.HasIndex(x => new { x.CourseId, x.Slug }).IsUnique();
        b.HasIndex(x => new { x.CourseId, x.Order }).IsUnique();
        b.HasIndex(x => new { x.CourseId, x.TranslationGroupId }).IsUnique().HasFilter("[TranslationGroupId] IS NOT NULL");
    }
}

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        ContentMapping.Configure(b);
        b.HasMany(x => x.Technologies).WithMany();
        b.HasMany(x => x.Screenshots).WithOne().HasForeignKey(x => x.ProjectId);
    }
}

public sealed class WorkshopConfiguration : IEntityTypeConfiguration<Workshop>
{
    public void Configure(EntityTypeBuilder<Workshop> b)
    {
        ContentMapping.Configure(b);
        b.Property(x => x.Price).HasPrecision(18, 2);
        b.HasMany(x => x.Sessions).WithOne().HasForeignKey(x => x.WorkshopId);
        b.ToTable("Workshops", t =>
        {
            t.HasCheckConstraint("CK_Workshop_Capacity", "[Capacity] > 0");
            t.HasCheckConstraint("CK_Workshop_Price", "[Price] >= 0");
        });
    }
}

public sealed class WorkshopSessionConfiguration : IEntityTypeConfiguration<WorkshopSession>
{
    public void Configure(EntityTypeBuilder<WorkshopSession> b)
    {
        b.HasIndex(x => new { x.WorkshopId, x.SessionNumber }).IsUnique();
        b.HasIndex(x => new { x.WorkshopId, x.TranslationGroupId }).IsUnique().HasFilter("[TranslationGroupId] IS NOT NULL");
    }
}
