using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PersonalBrand.Web.Pages;

public abstract class PublicPage : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Culture { get; set; } = "en";
    public bool Fa => Culture == "fa";

    public string T(string en, string fa) => Fa ? fa : en;
    public string StatusLabel(PersonalBrand.Core.ReservationStatus status) => status switch
    {
        PersonalBrand.Core.ReservationStatus.PendingPayment => T("Awaiting payment", "در انتظار پرداخت"),
        PersonalBrand.Core.ReservationStatus.PaymentSubmitted => T("Payment awaiting review", "پرداخت در انتظار بررسی"),
        PersonalBrand.Core.ReservationStatus.Confirmed => T("Confirmed", "تأیید شده"),
        PersonalBrand.Core.ReservationStatus.Rejected => T("Payment rejected", "پرداخت رد شده"),
        PersonalBrand.Core.ReservationStatus.Expired => T("Reservation expired", "مهلت رزرو تمام شده"),
        _ => T("Cancelled", "لغو شده")
    };
}

public static class Ui
{
    public static readonly string[] Sections = ["articles", "projects", "courses", "workshops"];
    public static string Label(string key, string culture) => culture == "fa" ? key switch
    {
        "articles" => "مقاله‌ها",
        "projects" => "پروژه‌ها",
        "courses" => "دوره‌های رایگان",
        "workshops" => "کارگاه‌ها",
        "about" => "درباره من",
        "resume" => "رزومه",
        "now" => "این روزها",
        "contact" => "تماس",
        "privacy" => "حریم خصوصی",
        _ => key
    } : char.ToUpperInvariant(key[0]) + key[1..];
}
