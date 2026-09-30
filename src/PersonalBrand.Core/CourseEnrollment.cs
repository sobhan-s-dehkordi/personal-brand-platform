using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Security.Cryptography;

namespace PersonalBrand.Core;

public sealed class CourseEnrollment : Entity
{
    public Guid CourseId
    {
        get; set;
    }
    public Course Course { get; set; } = null!;
    public Guid ContactId
    {
        get; set;
    }
    public Contact Contact { get; set; } = null!;
    public DateTimeOffset EnrolledAt
    {
        get; set;
    }
    public string Source { get; set; } = "course";
    public string Language { get; set; } = "en";
    public bool MarketingConsent
    {
        get; set;
    }
    public DateTimeOffset? MarketingConsentAt
    {
        get; set;
    }
    public string PrivacyPolicyVersion { get; set; } = "2026-09";
}
