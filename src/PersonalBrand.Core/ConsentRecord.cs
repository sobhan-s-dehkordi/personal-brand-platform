using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Security.Cryptography;

namespace PersonalBrand.Core;

public sealed class ConsentRecord : Entity
{
    public Guid ContactId
    {
        get; set;
    }
    public Contact Contact { get; set; } = null!;
    public ConsentType ConsentType
    {
        get; set;
    }
    public bool Granted
    {
        get; set;
    }
    public DateTimeOffset GrantedAt
    {
        get; set;
    }
    public DateTimeOffset? RevokedAt
    {
        get; set;
    }
    public string Source { get; set; } = "";
    public string PrivacyPolicyVersion { get; set; } = "2026-09";
}
