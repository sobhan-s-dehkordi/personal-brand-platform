using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace PersonalBrand.Infrastructure.Migrations
{
    /// <inheritdoc/>
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(name: "Articles", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), ReadingTime = table.Column<int>(type: "int", nullable: false), Language = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false), Slug = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false), TranslationGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true), Title = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false), Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false), Body = table.Column<string>(type: "nvarchar(max)", nullable: false), CoverImage = table.Column<string>(type: "nvarchar(max)", nullable: true), Status = table.Column<int>(type: "int", nullable: false), PublishedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true), UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), IsFeatured = table.Column<bool>(type: "bit", nullable: false), SeoTitle = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: true), SeoDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true) }, constraints: table =>
            {
                table.PrimaryKey("PK_Articles", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "AspNetRoles", columns: table => new { Id = table.Column<string>(type: "nvarchar(450)", nullable: false), Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true), NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true), ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true) }, constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "AspNetUsers", columns: table => new { Id = table.Column<string>(type: "nvarchar(450)", nullable: false), UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true), NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true), Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true), NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true), EmailConfirmed = table.Column<bool>(type: "bit", nullable: false), PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true), SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true), ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true), PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true), PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false), TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false), LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true), LockoutEnabled = table.Column<bool>(type: "bit", nullable: false), AccessFailedCount = table.Column<int>(type: "int", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "Contacts", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), FullName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false), Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false), NormalizedEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false), Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true), NormalizedMobile = table.Column<string>(type: "nvarchar(max)", nullable: true), TelegramUsername = table.Column<string>(type: "nvarchar(max)", nullable: true), PreferredLanguage = table.Column<string>(type: "nvarchar(max)", nullable: false), CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_Contacts", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "Courses", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), Difficulty = table.Column<string>(type: "nvarchar(max)", nullable: false), EstimatedDuration = table.Column<int>(type: "int", nullable: false), Language = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false), Slug = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false), TranslationGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true), Title = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false), Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false), Body = table.Column<string>(type: "nvarchar(max)", nullable: false), CoverImage = table.Column<string>(type: "nvarchar(max)", nullable: true), Status = table.Column<int>(type: "int", nullable: false), PublishedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true), UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), IsFeatured = table.Column<bool>(type: "bit", nullable: false), SeoTitle = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: true), SeoDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true) }, constraints: table =>
            {
                table.PrimaryKey("PK_Courses", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "Notifications", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), Channel = table.Column<string>(type: "nvarchar(max)", nullable: false), Recipient = table.Column<string>(type: "nvarchar(max)", nullable: false), Subject = table.Column<string>(type: "nvarchar(max)", nullable: false), Body = table.Column<string>(type: "nvarchar(max)", nullable: false), CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), SentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true), Attempts = table.Column<int>(type: "int", nullable: false), NextAttemptAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true) }, constraints: table =>
            {
                table.PrimaryKey("PK_Notifications", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "Projects", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), Problem = table.Column<string>(type: "nvarchar(max)", nullable: false), Solution = table.Column<string>(type: "nvarchar(max)", nullable: false), ArchitectureDescription = table.Column<string>(type: "nvarchar(max)", nullable: false), Challenges = table.Column<string>(type: "nvarchar(max)", nullable: false), Results = table.Column<string>(type: "nvarchar(max)", nullable: false), GitHubUrl = table.Column<string>(type: "nvarchar(max)", nullable: true), LiveDemoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true), DisplayOrder = table.Column<int>(type: "int", nullable: false), Language = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false), Slug = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false), TranslationGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true), Title = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false), Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false), Body = table.Column<string>(type: "nvarchar(max)", nullable: false), CoverImage = table.Column<string>(type: "nvarchar(max)", nullable: true), Status = table.Column<int>(type: "int", nullable: false), PublishedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true), UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), IsFeatured = table.Column<bool>(type: "bit", nullable: false), SeoTitle = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: true), SeoDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true) }, constraints: table =>
            {
                table.PrimaryKey("PK_Projects", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "Settings", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false), Value = table.Column<string>(type: "nvarchar(max)", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_Settings", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "Tag", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_Tag", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "Technology", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_Technology", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "Workshops", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), Capacity = table.Column<int>(type: "int", nullable: false), Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false), Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false), RegistrationOpensAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), RegistrationClosesAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), TimeZoneId = table.Column<string>(type: "nvarchar(max)", nullable: false), WorkshopStatus = table.Column<int>(type: "int", nullable: false), Prerequisites = table.Column<string>(type: "nvarchar(max)", nullable: false), WhatYouWillBuild = table.Column<string>(type: "nvarchar(max)", nullable: false), Audience = table.Column<string>(type: "nvarchar(max)", nullable: false), Platform = table.Column<string>(type: "nvarchar(max)", nullable: false), Language = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false), Slug = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false), TranslationGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true), Title = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false), Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false), Body = table.Column<string>(type: "nvarchar(max)", nullable: false), CoverImage = table.Column<string>(type: "nvarchar(max)", nullable: true), Status = table.Column<int>(type: "int", nullable: false), PublishedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true), UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), IsFeatured = table.Column<bool>(type: "bit", nullable: false), SeoTitle = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: true), SeoDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true) }, constraints: table =>
            {
                table.PrimaryKey("PK_Workshops", x => x.Id);
                table.CheckConstraint("CK_Workshop_Capacity", "[Capacity] > 0");
                table.CheckConstraint("CK_Workshop_Price", "[Price] >= 0");
            });
            migrationBuilder.CreateTable(name: "AspNetRoleClaims", columns: table => new { Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"), RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false), ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true), ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true) }, constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(name: "FK_AspNetRoleClaims_AspNetRoles_RoleId", column: x => x.RoleId, principalTable: "AspNetRoles", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateTable(name: "AspNetUserClaims", columns: table => new { Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"), UserId = table.Column<string>(type: "nvarchar(450)", nullable: false), ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true), ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true) }, constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(name: "FK_AspNetUserClaims_AspNetUsers_UserId", column: x => x.UserId, principalTable: "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateTable(name: "AspNetUserLogins", columns: table => new { LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false), ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false), ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true), UserId = table.Column<string>(type: "nvarchar(450)", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(name: "FK_AspNetUserLogins_AspNetUsers_UserId", column: x => x.UserId, principalTable: "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateTable(name: "AspNetUserRoles", columns: table => new { UserId = table.Column<string>(type: "nvarchar(450)", nullable: false), RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(name: "FK_AspNetUserRoles_AspNetRoles_RoleId", column: x => x.RoleId, principalTable: "AspNetRoles", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey(name: "FK_AspNetUserRoles_AspNetUsers_UserId", column: x => x.UserId, principalTable: "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateTable(name: "AspNetUserTokens", columns: table => new { UserId = table.Column<string>(type: "nvarchar(450)", nullable: false), LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false), Name = table.Column<string>(type: "nvarchar(450)", nullable: false), Value = table.Column<string>(type: "nvarchar(max)", nullable: true) }, constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(name: "FK_AspNetUserTokens_AspNetUsers_UserId", column: x => x.UserId, principalTable: "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateTable(name: "Consents", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), ContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), ConsentType = table.Column<int>(type: "int", nullable: false), Granted = table.Column<bool>(type: "bit", nullable: false), GrantedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), RevokedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true), Source = table.Column<string>(type: "nvarchar(max)", nullable: false), PrivacyPolicyVersion = table.Column<string>(type: "nvarchar(max)", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_Consents", x => x.Id);
                table.ForeignKey(name: "FK_Consents_Contacts_ContactId", column: x => x.ContactId, principalTable: "Contacts", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateTable(name: "Subscriptions", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), ContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), Status = table.Column<int>(type: "int", nullable: false), UnsubscribeToken = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false), CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), UnsubscribedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true) }, constraints: table =>
            {
                table.PrimaryKey("PK_Subscriptions", x => x.Id);
                table.ForeignKey(name: "FK_Subscriptions_Contacts_ContactId", column: x => x.ContactId, principalTable: "Contacts", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateTable(name: "ArticleCourse", columns: table => new { ArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), RelatedCoursesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_ArticleCourse", x => new { x.ArticleId, x.RelatedCoursesId });
                table.ForeignKey(name: "FK_ArticleCourse_Articles_ArticleId", column: x => x.ArticleId, principalTable: "Articles", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey(name: "FK_ArticleCourse_Courses_RelatedCoursesId", column: x => x.RelatedCoursesId, principalTable: "Courses", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateTable(name: "Enrollments", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), ContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), EnrolledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), Source = table.Column<string>(type: "nvarchar(max)", nullable: false), Language = table.Column<string>(type: "nvarchar(max)", nullable: false), MarketingConsent = table.Column<bool>(type: "bit", nullable: false), MarketingConsentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true), PrivacyPolicyVersion = table.Column<string>(type: "nvarchar(max)", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_Enrollments", x => x.Id);
                table.ForeignKey(name: "FK_Enrollments_Contacts_ContactId", column: x => x.ContactId, principalTable: "Contacts", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "FK_Enrollments_Courses_CourseId", column: x => x.CourseId, principalTable: "Courses", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            });
            migrationBuilder.CreateTable(name: "Lessons", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), Slug = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false), Title = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false), Order = table.Column<int>(type: "int", nullable: false), YouTubeVideoId = table.Column<string>(type: "nvarchar(max)", nullable: true), YouTubeUrl = table.Column<string>(type: "nvarchar(max)", nullable: true), Duration = table.Column<int>(type: "int", nullable: false), Summary = table.Column<string>(type: "nvarchar(max)", nullable: false), ContentBody = table.Column<string>(type: "nvarchar(max)", nullable: false), KeyPoints = table.Column<string>(type: "nvarchar(max)", nullable: false), Resources = table.Column<string>(type: "nvarchar(max)", nullable: false), GitHubUrl = table.Column<string>(type: "nvarchar(max)", nullable: true), IsPublished = table.Column<bool>(type: "bit", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_Lessons", x => x.Id);
                table.ForeignKey(name: "FK_Lessons_Courses_CourseId", column: x => x.CourseId, principalTable: "Courses", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateTable(name: "ProjectImage", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), Path = table.Column<string>(type: "nvarchar(max)", nullable: false), Alt = table.Column<string>(type: "nvarchar(max)", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_ProjectImage", x => x.Id);
                table.ForeignKey(name: "FK_ProjectImage_Projects_ProjectId", column: x => x.ProjectId, principalTable: "Projects", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateTable(name: "ArticleTag", columns: table => new { ArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), TagsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_ArticleTag", x => new { x.ArticleId, x.TagsId });
                table.ForeignKey(name: "FK_ArticleTag_Articles_ArticleId", column: x => x.ArticleId, principalTable: "Articles", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey(name: "FK_ArticleTag_Tag_TagsId", column: x => x.TagsId, principalTable: "Tag", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateTable(name: "ProjectTechnology", columns: table => new { ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), TechnologiesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_ProjectTechnology", x => new { x.ProjectId, x.TechnologiesId });
                table.ForeignKey(name: "FK_ProjectTechnology_Projects_ProjectId", column: x => x.ProjectId, principalTable: "Projects", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey(name: "FK_ProjectTechnology_Technology_TechnologiesId", column: x => x.TechnologiesId, principalTable: "Technology", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateTable(name: "Reservations", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), PublicReference = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false), AccessToken = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false), WorkshopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), ContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), Status = table.Column<int>(type: "int", nullable: false), ReservedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), HoldExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), PaymentSubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true), ConfirmedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true), CancelledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true), PriceAtReservationTime = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false), Currency = table.Column<string>(type: "nvarchar(max)", nullable: false), InternalNotes = table.Column<string>(type: "nvarchar(max)", nullable: false), RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_Reservations", x => x.Id);
                table.ForeignKey(name: "FK_Reservations_Contacts_ContactId", column: x => x.ContactId, principalTable: "Contacts", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "FK_Reservations_Workshops_WorkshopId", column: x => x.WorkshopId, principalTable: "Workshops", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            });
            migrationBuilder.CreateTable(name: "Sessions", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), WorkshopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), Title = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false), Description = table.Column<string>(type: "nvarchar(max)", nullable: false), SessionNumber = table.Column<int>(type: "int", nullable: false), StartAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), EndAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_Sessions", x => x.Id);
                table.ForeignKey(name: "FK_Sessions_Workshops_WorkshopId", column: x => x.WorkshopId, principalTable: "Workshops", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateTable(name: "Receipts", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), TrackingNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false), StorageKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false), OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false), ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false), FileSize = table.Column<long>(type: "bigint", nullable: false), UploadedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true), ReviewedByAdminId = table.Column<string>(type: "nvarchar(max)", nullable: true), ReviewNotes = table.Column<string>(type: "nvarchar(max)", nullable: true) }, constraints: table =>
            {
                table.PrimaryKey("PK_Receipts", x => x.Id);
                table.ForeignKey(name: "FK_Receipts_Reservations_ReservationId", column: x => x.ReservationId, principalTable: "Reservations", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateTable(name: "ReservationHistory", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), Action = table.Column<string>(type: "nvarchar(max)", nullable: false), OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), ActorType = table.Column<string>(type: "nvarchar(max)", nullable: false), ActorIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: false), Note = table.Column<string>(type: "nvarchar(max)", nullable: true) }, constraints: table =>
            {
                table.PrimaryKey("PK_ReservationHistory", x => x.Id);
                table.ForeignKey(name: "FK_ReservationHistory_Reservations_ReservationId", column: x => x.ReservationId, principalTable: "Reservations", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateIndex(name: "IX_ArticleCourse_RelatedCoursesId", table: "ArticleCourse", column: "RelatedCoursesId");
            migrationBuilder.CreateIndex(name: "IX_Articles_Language_Slug", table: "Articles", columns: new[] { "Language", "Slug" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_Articles_Status_PublishedAt", table: "Articles", columns: new[] { "Status", "PublishedAt" });
            migrationBuilder.CreateIndex(name: "IX_Articles_TranslationGroupId", table: "Articles", column: "TranslationGroupId");
            migrationBuilder.CreateIndex(name: "IX_ArticleTag_TagsId", table: "ArticleTag", column: "TagsId");
            migrationBuilder.CreateIndex(name: "IX_AspNetRoleClaims_RoleId", table: "AspNetRoleClaims", column: "RoleId");
            migrationBuilder.CreateIndex(name: "RoleNameIndex", table: "AspNetRoles", column: "NormalizedName", unique: true, filter: "[NormalizedName] IS NOT NULL");
            migrationBuilder.CreateIndex(name: "IX_AspNetUserClaims_UserId", table: "AspNetUserClaims", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_AspNetUserLogins_UserId", table: "AspNetUserLogins", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_AspNetUserRoles_RoleId", table: "AspNetUserRoles", column: "RoleId");
            migrationBuilder.CreateIndex(name: "EmailIndex", table: "AspNetUsers", column: "NormalizedEmail");
            migrationBuilder.CreateIndex(name: "UserNameIndex", table: "AspNetUsers", column: "NormalizedUserName", unique: true, filter: "[NormalizedUserName] IS NOT NULL");
            migrationBuilder.CreateIndex(name: "IX_Consents_ContactId_ConsentType", table: "Consents", columns: new[] { "ContactId", "ConsentType" });
            migrationBuilder.CreateIndex(name: "IX_Contacts_NormalizedEmail", table: "Contacts", column: "NormalizedEmail", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Courses_Language_Slug", table: "Courses", columns: new[] { "Language", "Slug" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_Courses_Status_PublishedAt", table: "Courses", columns: new[] { "Status", "PublishedAt" });
            migrationBuilder.CreateIndex(name: "IX_Courses_TranslationGroupId", table: "Courses", column: "TranslationGroupId");
            migrationBuilder.CreateIndex(name: "IX_Enrollments_ContactId", table: "Enrollments", column: "ContactId");
            migrationBuilder.CreateIndex(name: "IX_Enrollments_CourseId_ContactId", table: "Enrollments", columns: new[] { "CourseId", "ContactId" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_Enrollments_EnrolledAt", table: "Enrollments", column: "EnrolledAt");
            migrationBuilder.CreateIndex(name: "IX_Lessons_CourseId_Order", table: "Lessons", columns: new[] { "CourseId", "Order" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_Lessons_CourseId_Slug", table: "Lessons", columns: new[] { "CourseId", "Slug" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_Notifications_SentAt_NextAttemptAt", table: "Notifications", columns: new[] { "SentAt", "NextAttemptAt" });
            migrationBuilder.CreateIndex(name: "IX_ProjectImage_ProjectId", table: "ProjectImage", column: "ProjectId");
            migrationBuilder.CreateIndex(name: "IX_Projects_Language_Slug", table: "Projects", columns: new[] { "Language", "Slug" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_Projects_Status_PublishedAt", table: "Projects", columns: new[] { "Status", "PublishedAt" });
            migrationBuilder.CreateIndex(name: "IX_Projects_TranslationGroupId", table: "Projects", column: "TranslationGroupId");
            migrationBuilder.CreateIndex(name: "IX_ProjectTechnology_TechnologiesId", table: "ProjectTechnology", column: "TechnologiesId");
            migrationBuilder.CreateIndex(name: "IX_Receipts_ReservationId", table: "Receipts", column: "ReservationId");
            migrationBuilder.CreateIndex(name: "IX_ReservationHistory_ReservationId", table: "ReservationHistory", column: "ReservationId");
            migrationBuilder.CreateIndex(name: "IX_Reservations_AccessToken", table: "Reservations", column: "AccessToken", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Reservations_ContactId", table: "Reservations", column: "ContactId");
            migrationBuilder.CreateIndex(name: "IX_Reservations_HoldExpiresAt", table: "Reservations", column: "HoldExpiresAt");
            migrationBuilder.CreateIndex(name: "IX_Reservations_PublicReference", table: "Reservations", column: "PublicReference", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Reservations_WorkshopId_Status_HoldExpiresAt", table: "Reservations", columns: new[] { "WorkshopId", "Status", "HoldExpiresAt" });
            migrationBuilder.CreateIndex(name: "IX_Sessions_WorkshopId_SessionNumber", table: "Sessions", columns: new[] { "WorkshopId", "SessionNumber" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_Settings_Key", table: "Settings", column: "Key", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Subscriptions_ContactId", table: "Subscriptions", column: "ContactId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Subscriptions_UnsubscribeToken", table: "Subscriptions", column: "UnsubscribeToken", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Workshops_Language_Slug", table: "Workshops", columns: new[] { "Language", "Slug" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_Workshops_Status_PublishedAt", table: "Workshops", columns: new[] { "Status", "PublishedAt" });
            migrationBuilder.CreateIndex(name: "IX_Workshops_TranslationGroupId", table: "Workshops", column: "TranslationGroupId");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ArticleCourse");
            migrationBuilder.DropTable(name: "ArticleTag");
            migrationBuilder.DropTable(name: "AspNetRoleClaims");
            migrationBuilder.DropTable(name: "AspNetUserClaims");
            migrationBuilder.DropTable(name: "AspNetUserLogins");
            migrationBuilder.DropTable(name: "AspNetUserRoles");
            migrationBuilder.DropTable(name: "AspNetUserTokens");
            migrationBuilder.DropTable(name: "Consents");
            migrationBuilder.DropTable(name: "Enrollments");
            migrationBuilder.DropTable(name: "Lessons");
            migrationBuilder.DropTable(name: "Notifications");
            migrationBuilder.DropTable(name: "ProjectImage");
            migrationBuilder.DropTable(name: "ProjectTechnology");
            migrationBuilder.DropTable(name: "Receipts");
            migrationBuilder.DropTable(name: "ReservationHistory");
            migrationBuilder.DropTable(name: "Sessions");
            migrationBuilder.DropTable(name: "Settings");
            migrationBuilder.DropTable(name: "Subscriptions");
            migrationBuilder.DropTable(name: "Articles");
            migrationBuilder.DropTable(name: "Tag");
            migrationBuilder.DropTable(name: "AspNetRoles");
            migrationBuilder.DropTable(name: "AspNetUsers");
            migrationBuilder.DropTable(name: "Courses");
            migrationBuilder.DropTable(name: "Projects");
            migrationBuilder.DropTable(name: "Technology");
            migrationBuilder.DropTable(name: "Reservations");
            migrationBuilder.DropTable(name: "Contacts");
            migrationBuilder.DropTable(name: "Workshops");
        }
    }
}
