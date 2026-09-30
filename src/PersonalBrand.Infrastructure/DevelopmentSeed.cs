using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalBrand.Core;

namespace PersonalBrand.Infrastructure;

public static class DevelopmentSeed
{
    public static async Task CreateAdminAsync(IServiceProvider services, IConfiguration config)
    {
        var email = config["AdminSeed:Email"] ?? throw new InvalidOperationException("Set AdminSeed:Email in secrets.");
        var password = config["AdminSeed:Password"] ?? throw new InvalidOperationException("Set AdminSeed:Password in secrets.");
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var users = services.GetRequiredService<UserManager<IdentityUser>>();
        if (!await roles.RoleExistsAsync("Administrator"))
            Ensure(await roles.CreateAsync(new IdentityRole("Administrator")));
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };
            Ensure(await users.CreateAsync(user, password));
        }

        if (!await users.IsInRoleAsync(user, "Administrator"))
            Ensure(await users.AddToRoleAsync(user, "Administrator"));
    }

    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
    }

    public static async Task RunAsync(BrandDbContext db)
    {
        if (await db.Articles.AnyAsync())
            return;
        var now = DateTimeOffset.UtcNow;
        var articleGroup = Guid.NewGuid();
        var courseGroup = Guid.NewGuid();
        var projectGroup = Guid.NewGuid();
        var workshopGroup = Guid.NewGuid();
        var lessonGroups = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var sessionGroups = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToArray();
        foreach (var language in new[]
        {
            "en",
            "fa"
        }

        )
        {
            var fa = language == "fa";
            var course = new Course
            {
                Language = language,
                TranslationGroupId = courseGroup,
                Slug = "practical-dotnet",
                Title = fa ? "دات‌نت کاربردی" : "Practical .NET foundations",
                Summary = fa ? "ساخت سرویس‌های قابل نگهداری با سی‌شارپ و دات‌نت." : "Build maintainable services with C# and .NET.",
                Body = fa ? "## از یک مسئله واقعی شروع کنید\nدامنه را از جزئیات ذخیره‌سازی جدا کنید." : "## Start with a real problem\nKeep domain rules independent of persistence. This free course explores the boundaries that make an application easy to change.",
                Status = ContentStatus.Published,
                PublishedAt = now,
                UpdatedAt = now,
                EstimatedDuration = 90,
                IsFeatured = true
            };
            course.Lessons.Add(new Lesson { TranslationGroupId = lessonGroups[0], Slug = "domain-boundaries", Title = fa ? "مرزهای دامنه" : "Domain boundaries", Order = 1, Duration = 25, IsPublished = true, Summary = fa ? "قواعد کسب‌وکار مستقل از پایگاه داده هستند." : "Business rules belong in the domain, independent of the database.", ContentBody = fa ? "## یک قاعده ساده\nظرفیت کارگاه نباید منفی باشد. این قاعده باید در تمام مسیرهای برنامه اعمال شود." : "## One simple rule\nA workshop cannot have negative capacity. Express that invariant in the domain and enforce it in every write path.\n\n```csharp\nif (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));\n```", KeyPoints = fa ? "- قواعد را صریح بنویسید\n- ورودی را بررسی کنید" : "- Make invariants explicit\n- Validate boundaries", Resources = "[.NET documentation](https://learn.microsoft.com/dotnet/)" });
            course.Lessons.Add(new Lesson { TranslationGroupId = lessonGroups[1], Slug = "persistence", Title = fa ? "ذخیره‌سازی و تراکنش" : "Persistence and transactions", Order = 2, Duration = 30, IsPublished = true, Summary = fa ? "چرا تراکنش بخشی از صحت برنامه است؟" : "Why transaction boundaries are part of correctness.", ContentBody = fa ? "خواندن ظرفیت و ایجاد رزرو باید در یک تراکنش محافظت‌شده انجام شود." : "Reading capacity and inserting a reservation must happen inside one protected transaction. A check in the browser cannot guarantee availability.", KeyPoints = "- Atomicity\n- Concurrency\n- SQL Server locks", Resources = "[EF Core](https://learn.microsoft.com/ef/core/)" });
            db.Courses.Add(course);
            var article = new Article
            {
                Language = language,
                TranslationGroupId = articleGroup,
                Slug = "boundaries-before-frameworks",
                Title = fa ? "مرزها پیش از فریم‌ورک‌ها" : "Boundaries before frameworks",
                Summary = fa ? "یادداشتی درباره تصمیم‌های کوچک که تغییر نرم‌افزار را ساده می‌کنند." : "Small design decisions that make software easier to change.",
                Body = fa ? "## وابستگی‌ها را آگاهانه انتخاب کنید\nقواعد اصلی برنامه نباید به چارچوب وب وابسته باشند.\n\n## سادگی\nیک برنامه یکپارچه با مرزهای روشن، شروع مناسبی است." : "## Choose dependencies deliberately\nCore business rules should not depend on a web framework. Persistence and delivery mechanisms can change while the domain stays stable.\n\n## Keep it small\nA modular monolith gives you clear boundaries without distributing every interaction.\n\n```csharp\npublic bool HasCapacity(int occupied) => occupied < Capacity;\n```",
                Status = ContentStatus.Published,
                PublishedAt = now,
                UpdatedAt = now,
                IsFeatured = true,
                ReadingTime = 4
            };
            article.Tags.Add(new Tag { Name = fa ? "معماری" : "Architecture" });
            article.RelatedCourses.Add(course);
            db.Articles.Add(article);
            var project = new Project
            {
                Language = language,
                TranslationGroupId = projectGroup,
                Slug = "learning-platform",
                Title = fa ? "پلتفرم آموزش و مهندسی" : "An engineering & learning platform",
                Summary = fa ? "نمونه توسعه: محتوای دو‌زبانه، دوره‌های آزاد و رزرو کارگاه." : "Development example: bilingual publishing, open courses, and workshop reservations.",
                Body = fa ? "این نمونه توسعه برای نمایش قابلیت‌های پلتفرم است." : "This development example demonstrates the platform. It is not a claim about a client engagement.",
                Problem = fa ? "حفظ دسترسی آزاد به آموزش و مدیریت ظرفیت محدود." : "Keep learning open while managing limited live workshop capacity.",
                Solution = fa ? "دو برنامه مستقل برای بازدیدکنندگان و مدیران." : "Separate public and administration applications over a shared domain.",
                ArchitectureDescription = "Core → Infrastructure adapters → Razor Pages",
                Challenges = fa ? "درخواست‌های همزمان رزرو." : "Concurrent reservations and private payment receipts.",
                Results = fa ? "نمونه قابل بررسی برای توسعه محلی." : "A local development example ready for inspection.",
                Status = ContentStatus.Published,
                PublishedAt = now,
                UpdatedAt = now,
                IsFeatured = true
            };
            project.Technologies.Add(new Technology { Name = ".NET 10" });
            project.Technologies.Add(new Technology { Name = "SQL Server" });
            db.Projects.Add(project);
            var workshop = new Workshop
            {
                Language = language,
                TranslationGroupId = workshopGroup,
                Slug = "build-with-dotnet",
                Title = fa ? "ساخت یک سرویس با دات‌نت" : "Build a service with .NET",
                Summary = fa ? "کارگاه نمونه توسعه؛ پیش از انتشار اطلاعات واقعی را وارد کنید." : "Development workshop example. Configure real details before publishing.",
                Body = fa ? "از مدل دامنه تا استقرار، یک سرویس کوچک می‌سازیم." : "Build a small service from domain modeling through deployment.",
                Capacity = 7,
                Price = 25000000,
                Currency = "IRR",
                RegistrationOpensAt = now.AddDays(-1),
                RegistrationClosesAt = now.AddDays(14),
                StartDate = now.AddDays(15),
                EndDate = now.AddDays(43),
                WorkshopStatus = WorkshopStatus.Open,
                Status = ContentStatus.Published,
                PublishedAt = now,
                UpdatedAt = now,
                Audience = fa ? "توسعه‌دهندگان آشنا با سی‌شارپ" : "Developers familiar with C#",
                Prerequisites = "C#, HTTP, SQL",
                WhatYouWillBuild = fa ? "یک سرویس با قواعد دامنه و تست" : "A service with domain rules and tests",
                Platform = fa ? "پیوند جلسه پس از تأیید ارسال می‌شود" : "Meeting details shared after confirmation"
            };
            for (var i = 0; i < 5; i++)
                workshop.Sessions.Add(new WorkshopSession { TranslationGroupId = sessionGroups[i], Description = fa ? "توضیحات جلسه" : "Session description", Title = fa ? $"جلسه {i + 1}" : $"Session {i + 1}", SessionNumber = i + 1, StartAt = now.AddDays(15 + i * 7), EndAt = now.AddDays(15 + i * 7).AddHours(2) });
            db.Workshops.Add(workshop);
            foreach (var page in new[]
            {
                "about",
                "resume",
                "now",
                "contact",
                "privacy"
            }

            )
            {
                var body = page switch
                {
                    "privacy" => fa ? "## حریم خصوصی — نسخه 2026-09\nاطلاعات تماس برای ثبت‌نام دوره و اجرای کارگاه نگهداری می‌شود. دریافت خبر فقط با رضایت شما انجام می‌شود و هر زمان از پیوند لغو عضویت در ایمیل قابل توقف است. لغو خبرنامه سوابق ضروری تراکنش را حذف نمی‌کند. رسیدها فقط برای مدیر مجاز قابل مشاهده‌اند. برای اصلاح یا حذف اطلاعات با مسئول سایت تماس بگیرید." : "## Privacy — version 2026-09\nContact details are stored to manage course enrollment and workshop operations. Promotional updates require your explicit consent. Use the unsubscribe link in any preference email to stop them. Unsubscribing does not remove necessary transaction records. Only authorized administrators can view receipts. Contact the site operator to request correction or deletion. The operator must define applicable retention periods before launch.",
                    "now" => fa ? "## در حال ساخت\nابزارهای آموزشی کاربردی.\n## در حال یادگیری\nمعماری نرم‌افزار و مهندسی هوش مصنوعی." : "## Building\nPractical learning tools.\n## Learning\nSoftware architecture and AI engineering.\n## Writing\nNotes on maintainable .NET applications.\n## Exploring\nUseful AI integrations.",
                    "resume" => fa ? "## مهارت‌ها\n.NET، سی‌شارپ، SQL Server و طراحی سرویس.\n\nسوابق، تحصیلات و فایل رزومه را پیش از انتشار در تنظیمات تکمیل کنید." : "## Skills\n.NET, C#, SQL Server, and service design.\n\nComplete verified experience, education, and the CV download in settings before publication.",
                    "about" => fa ? "## تمرکز فنی\nمهندسی بک‌اند، دات‌نت و معماری نرم‌افزار.\n## آموزش\nتوسعه کاربردی با مثال‌های قابل اجرا." : "## Technical focus\nBackend engineering, .NET, and software architecture.\n## Teaching\nPractical development through runnable examples.",
                    _ => fa ? "برای همکاری حرفه‌ای یا پرسش درباره کارگاه از پیوندهای زیر استفاده کنید." : "For professional collaboration or workshop questions, use the configured links below."
                };
                db.Settings.Add(new SiteSetting { Key = $"Page.{page}.{language}", Value = body });
            }
        }

        await db.SaveChangesAsync();
    }
}
