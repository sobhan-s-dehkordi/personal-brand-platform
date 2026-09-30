using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PersonalBrand.Core;

namespace PersonalBrand.Infrastructure;

public sealed class BrandDbContext(DbContextOptions<BrandDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Workshop> Workshops => Set<Workshop>();
    public DbSet<WorkshopSession> Sessions => Set<WorkshopSession>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<CourseEnrollment> Enrollments => Set<CourseEnrollment>();
    public DbSet<WorkshopReservation> Reservations => Set<WorkshopReservation>();
    public DbSet<PaymentReceipt> Receipts => Set<PaymentReceipt>();
    public DbSet<ConsentRecord> Consents => Set<ConsentRecord>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SiteSetting> Settings => Set<SiteSetting>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Ignore<Content>();
        builder.ApplyConfigurationsFromAssembly(typeof(BrandDbContext).Assembly);
        // Domain entities assign their own IDs, including children added to tracked aggregates.
        foreach (var type in builder.Model.GetEntityTypes().Where(t => typeof(Entity).IsAssignableFrom(t.ClrType)).ToList())
            builder.Entity(type.ClrType).Property(nameof(Entity.Id)).ValueGeneratedNever();
    }
}

public sealed class DesignFactory : IDesignTimeDbContextFactory<BrandDbContext>
{
    public BrandDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<BrandDbContext>().UseSqlServer(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ?? "Server=.;Database=PersonalBrandDevelopment;Integrated Security=True;TrustServerCertificate=True").Options);
}
