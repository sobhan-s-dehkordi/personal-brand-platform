using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalBrand.Core;

namespace PersonalBrand.Infrastructure.Configurations;

public sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> b)
    {
        b.HasIndex(x => x.NormalizedEmail).IsUnique();
        b.Property(x => x.NormalizedEmail).HasMaxLength(254);
        b.Property(x => x.Email).HasMaxLength(254);
        b.Property(x => x.FullName).HasMaxLength(160);
    }
}

public sealed class CourseEnrollmentConfiguration : IEntityTypeConfiguration<CourseEnrollment>
{
    public void Configure(EntityTypeBuilder<CourseEnrollment> b)
    {
        b.HasIndex(x => new { x.CourseId, x.ContactId }).IsUnique();
        b.HasIndex(x => x.EnrolledAt);
        b.HasOne(x => x.Course).WithMany().HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Contact).WithMany().HasForeignKey(x => x.ContactId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ConsentRecordConfiguration : IEntityTypeConfiguration<ConsentRecord>
{
    public void Configure(EntityTypeBuilder<ConsentRecord> b)
    {
        b.HasIndex(x => new { x.ContactId, x.ConsentType });
        b.HasOne(x => x.Contact).WithMany().HasForeignKey(x => x.ContactId);
    }
}

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> b)
    {
        b.HasIndex(x => x.ContactId).IsUnique();
        b.HasIndex(x => x.UnsubscribeToken).IsUnique();
        b.Property(x => x.UnsubscribeToken).HasMaxLength(64);
        b.HasOne(x => x.Contact).WithMany().HasForeignKey(x => x.ContactId);
    }
}

public sealed class WorkshopReservationConfiguration : IEntityTypeConfiguration<WorkshopReservation>
{
    public void Configure(EntityTypeBuilder<WorkshopReservation> b)
    {
        b.HasIndex(x => x.PublicReference).IsUnique();
        b.Property(x => x.PublicReference).HasMaxLength(40);
        b.HasIndex(x => x.AccessToken).IsUnique();
        b.Property(x => x.AccessToken).HasMaxLength(64);
        b.HasIndex(x => new { x.WorkshopId, x.Status, x.HoldExpiresAt });
        b.HasIndex(x => x.HoldExpiresAt);
        b.Property(x => x.PriceAtReservationTime).HasPrecision(18, 2);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasOne(x => x.Workshop).WithMany().HasForeignKey(x => x.WorkshopId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Contact).WithMany().HasForeignKey(x => x.ContactId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Receipts).WithOne().HasForeignKey(x => x.ReservationId);
        b.HasMany(x => x.History).WithOne().HasForeignKey(x => x.ReservationId);
    }
}

public sealed class PaymentReceiptConfiguration : IEntityTypeConfiguration<PaymentReceipt>
{
    public void Configure(EntityTypeBuilder<PaymentReceipt> b)
    {
        b.Property(x => x.TrackingNumber).HasMaxLength(80);
        b.Property(x => x.StorageKey).HasMaxLength(100);
    }
}

public sealed class SettingConfiguration : IEntityTypeConfiguration<SiteSetting>
{
    public void Configure(EntityTypeBuilder<SiteSetting> b)
    {
        b.HasIndex(x => x.Key).IsUnique();
        b.Property(x => x.Key).HasMaxLength(100);
    }
}

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.HasIndex(x => new { x.SentAt, x.NextAttemptAt });
    }
}
