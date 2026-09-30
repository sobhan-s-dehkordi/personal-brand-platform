using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Security.Cryptography;

namespace PersonalBrand.Core;

public sealed class Contact : Entity
{
    public string FullName { get; set; } = "";
    public string Email { get; private set; } = "";
    public string NormalizedEmail { get; private set; } = "";
    public string? Mobile
    {
        get; set;
    }
    public string? NormalizedMobile
    {
        get; set;
    }
    public string? TelegramUsername
    {
        get; set;
    }
    public string PreferredLanguage { get; set; } = "en";
    public DateTimeOffset CreatedAt
    {
        get; set;
    }
    public DateTimeOffset UpdatedAt
    {
        get; set;
    }

    public static string NormalizeEmail(string email)
    {
        var value = email.Trim();
        if (value.Length > 254 || !MailAddress.TryCreate(value, out var parsed) || parsed.Address != value)
            throw new DomainException("Enter a valid email address.");
        return value.ToUpperInvariant();
    }

    public void SetEmail(string email)
    {
        NormalizedEmail = NormalizeEmail(email);
        Email = email.Trim();
    }
}
