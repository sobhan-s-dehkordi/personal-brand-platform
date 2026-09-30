using Microsoft.EntityFrameworkCore;
using PersonalBrand.Core;

namespace PersonalBrand.Infrastructure;

public record SiteContentField(string Key, string Group, string Label, string English, string Persian, bool Rich = false);
public sealed class SiteContent(BrandDbContext db)
{
    public static readonly SiteContentField[] Fields = [
        new("Brand.Name", "Brand and contact", "Name", "Sobhan Soleiman", "سبحان سلیمان"),
        new("Brand.ProfessionalTitle", "Brand and contact", "Professional Title", "Backend Engineer", "توسعه‌دهندهٔ بک‌اند"),
        new("Brand.ShortBio", "Brand and contact", "Short Bio", "I build software, write about engineering, and teach practical development.", "درباره مهندسی نرم‌افزار می‌نویسم، محصول می‌سازم و توسعه کاربردی آموزش می‌دهم.", true),
        new("Brand.DefaultSeoDescription", "Brand and contact", "Default Seo Description", ".NET, AI, and software architecture. Projects, articles, and practical courses.", "پروژه‌ها، مقاله‌ها و آموزش‌های کاربردی توسعه نرم‌افزار."),
        new("Brand.ContactEmail", "Brand and contact", "Contact Email", "", ""),
        new("Brand.ProfileImage", "Brand and contact", "Profile Image", "", ""),
        new("Brand.ResumePath", "Brand and contact", "Resume Path", "", ""),
        new("Brand.GitHubUrl", "Brand and contact", "Git Hub Url", "", ""),
        new("Brand.LinkedInUrl", "Brand and contact", "Linked In Url", "", ""),
        new("Brand.YouTubeUrl", "Brand and contact", "You Tube Url", "", ""),
        new("Home.Title", "Homepage content", "Title", "Engineering, in practice", "مهندسی در عمل"),
        new("Home.Eyebrow", "Homepage content", "Eyebrow", ".NET / AI / SOFTWARE ARCHITECTURE", "دات‌نت / هوش مصنوعی / معماری نرم‌افزار"),
        new("Home.Heading", "Homepage content", "Heading", "Hi, I'm Sobhan Soleiman.", "سلام، من سبحان سلیمان هستم."),
        new("Home.Highlight", "Homepage content", "Highlight", "I build useful software.", "نرم‌افزار کاربردی می‌سازم."),
        new("Home.Note", "Homepage content", "Note", "Building • Writing • Teaching", "ساختن • نوشتن • آموزش"),
        new("Home.AboutEyebrow", "Homepage content", "About Eyebrow", "BEHIND THE WORK", "پشت صحنه کار"),
        new("Home.AboutHeading", "Homepage content", "About Heading", "Engineering, with a practical focus.", "مهندسی با تمرکز بر کاربرد."),
        new("Home.AboutBody", "Homepage content", "About Body", "I focus on backend systems, .NET, and useful AI integrations. This is where I share what I build and learn.", "تمرکز من بر سیستم‌های بک‌اند، دات‌نت و کاربردهای مفید هوش مصنوعی است. اینجا از ساختن و یادگرفتن می‌نویسم.", true),
        new("Home.NewsletterHeading", "Homepage content", "Newsletter Heading", "Notes from the workbench.", "یادداشت‌های مسیر ساخت."),
        new("Home.NewsletterBody", "Homepage content", "Newsletter Body", "New articles, practical courses, and small live workshops.", "مقاله‌های تازه، دوره‌های کاربردی و کارگاه‌های کوچک.", true),
        new("Footer.Text", "Footer content", "Text", "Sobhan Soleiman · .NET / AI / Architecture", "سبحان سلیمان · دات‌نت / هوش مصنوعی / معماری")
    ];
    private static readonly SiteContentField[] InterfaceText = [
        new("Home.ProjectsButton", "دکمه‌ها و بخش‌ها", "دکمهٔ پروژه‌ها", "Explore projects ↗", "پروژه‌ها ↖"),
        new("Home.ArticlesButton", "دکمه‌ها و بخش‌ها", "دکمهٔ مقاله‌ها", "Read the journal", "خواندن مقاله‌ها"),
        new("Home.ResumeButton", "دکمه‌ها و بخش‌ها", "دکمهٔ رزومه", "Resume", "رزومه"),
        new("Home.ViewAll", "دکمه‌ها و بخش‌ها", "مشاهدهٔ همه", "View all →", "مشاهده همه ←"),
        new("Home.Empty", "دکمه‌ها و بخش‌ها", "پیام پیش‌فرض بخش خالی", "New work will appear here soon.", "مطالب تازه به‌زودی اینجا منتشر می‌شود.", true),
        new("Home.AboutButton", "بخش دربارهٔ من", "دکمهٔ بیشتر بخوانید", "More about me →", "درباره من ←"),
        new("Home.NameLabel", "خبرنامه", "برچسب نام", "Name (optional)", "نام (اختیاری)"),
        new("Home.EmailLabel", "خبرنامه", "برچسب ایمیل", "Email", "ایمیل"),
        new("Home.Consent", "خبرنامه", "متن رضایت", "Send me educational updates, articles, and workshop announcements. I can unsubscribe anytime.", "مایلم اخبار آموزشی، مقاله‌ها و کارگاه‌ها را دریافت کنم. هر زمان امکان لغو عضویت دارم."),
        new("Home.Subscribe", "خبرنامه", "دکمهٔ عضویت", "Subscribe", "عضویت"),
        new("Home.Subscribed", "خبرنامه", "پیام عضویت موفق", "Your update preferences are saved.", "تنظیمات دریافت خبر ذخیره شد."),
        new("Home.SubscriberName", "خبرنامه", "نام پیش‌فرض مشترک", "Subscriber", "مشترک"),
        new("Home.SubscribeError", "خبرنامه", "پیام خطای ثبت‌نام", "Please check your details and try again.", "اطلاعات واردشده را بررسی کنید و دوباره تلاش کنید."),
        new("Navigation.Skip", "منو و پایین صفحه", "رفتن به محتوا", "Skip to content", "رفتن به محتوا"),
        new("Navigation.Label", "منو و پایین صفحه", "نام منوی اصلی", "Main navigation", "منوی اصلی"),
        new("Navigation.Switch", "منو و پایین صفحه", "دکمهٔ تغییر زبان", "فارسی ↗", "EN ↗"),
        new("Navigation.articles", "منو و پایین صفحه", "مقاله‌ها", "Articles", "مقاله‌ها"),
        new("Navigation.projects", "منو و پایین صفحه", "پروژه‌ها", "Projects", "پروژه‌ها"),
        new("Navigation.courses", "منو و پایین صفحه", "دوره‌های رایگان", "Free courses", "دوره‌های رایگان"),
        new("Navigation.workshops", "منو و پایین صفحه", "کارگاه‌ها", "Workshops", "کارگاه‌ها"),
        new("Navigation.about", "منو و پایین صفحه", "درباره من", "About", "درباره من"),
        new("Navigation.resume", "منو و پایین صفحه", "رزومه", "Resume", "رزومه"),
        new("Navigation.now", "منو و پایین صفحه", "این روزها", "Now", "این روزها"),
        new("Navigation.contact", "منو و پایین صفحه", "تماس", "Contact", "تماس"),
        new("Navigation.privacy", "منو و پایین صفحه", "حریم خصوصی", "Privacy policy", "حریم خصوصی"),
        new("Social.GitHub", "ارتباط و پیوندها", "عنوان GitHub", "GitHub", "GitHub"),
        new("Social.LinkedIn", "ارتباط و پیوندها", "عنوان LinkedIn", "LinkedIn", "LinkedIn"),
        new("Social.YouTube", "ارتباط و پیوندها", "عنوان YouTube", "YouTube", "YouTube")
    ];
    private Dictionary<string, string>? values;
    public async Task LoadAsync(CancellationToken ct = default)
    {
        values ??= await db.Settings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value, ct);
    }

    public string Get(string key, string language)
    {
        var fixedText = InterfaceText.FirstOrDefault(x => x.Key == key);
        if (fixedText is not null)
            return language == "fa" ? fixedText.Persian : fixedText.English;
        var field = Fields.FirstOrDefault(x => x.Key == key);
        return values?.GetValueOrDefault(key + "." + language) ?? (language == "fa" ? field?.Persian : field?.English) ?? "";
    }
    public async Task EnsureDefaultsAsync(CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await SqlLocks.AcquireAsync(db, "site-content-defaults", ct);
        var keys = (await db.Settings.Select(x => x.Key).ToListAsync(ct)).ToHashSet();
        foreach (var field in Fields)
            foreach (var language in new[]
            {
                "en",
                "fa"
            }

            )
            {
                var key = field.Key + "." + language;
                if (!keys.Contains(key))
                    db.Settings.Add(new SiteSetting { Key = key, Value = language == "fa" ? field.Persian : field.English });
            }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task<BrandOptions> BrandAsync(string language)
    {
        await LoadAsync();
        string? Optional(string key) => string.IsNullOrWhiteSpace(Get(key, language)) ? null : Get(key, language);
        return new BrandOptions
        {
            Name = Get("Brand.Name", language),
            ProfessionalTitle = Get("Brand.ProfessionalTitle", language),
            ShortBio = Get("Brand.ShortBio", language),
            ContactEmail = Get("Brand.ContactEmail", language),
            DefaultSeoDescription = Get("Brand.DefaultSeoDescription", language),
            GitHubUrl = Optional("Brand.GitHubUrl"),
            LinkedInUrl = Optional("Brand.LinkedInUrl"),
            YouTubeUrl = Optional("Brand.YouTubeUrl"),
            ProfileImage = Optional("Brand.ProfileImage"),
            ResumePath = Optional("Brand.ResumePath")
        };
    }
}
