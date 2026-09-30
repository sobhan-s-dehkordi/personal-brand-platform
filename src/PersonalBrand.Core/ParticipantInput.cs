using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Security.Cryptography;

namespace PersonalBrand.Core;

public sealed class ParticipantInput
{
    [Required, StringLength(160)]
    public string FullName { get; set; } = "";

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";

    [RegularExpression(@"^\+?[0-9 ()-]{7,24}$")]
    public string? Mobile
    {
        get; set;
    }

    [RegularExpression(@"^@?[A-Za-z0-9_]{5,32}$")]
    public string? TelegramUsername
    {
        get; set;
    }

    [RegularExpression("^(en|fa)$")]
    public string Language { get; set; } = "en";
    public bool MarketingConsent
    {
        get; set;
    }

    public void Validate() => Validator.ValidateObject(this, new ValidationContext(this), true);
}
