using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Security.Cryptography;

namespace PersonalBrand.Core;

public sealed class Subscription : Entity
{
    public Guid ContactId
    {
        get; set;
    }
    public Contact Contact { get; set; } = null!;
    public SubscriptionStatus Status { get; private set; } = SubscriptionStatus.Active;
    public string UnsubscribeToken { get; private set; } = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    public DateTimeOffset CreatedAt
    {
        get; set;
    }
    public DateTimeOffset? UnsubscribedAt
    {
        get; private set;
    }

    public void Unsubscribe(DateTimeOffset now)
    {
        Status = SubscriptionStatus.Unsubscribed;
        UnsubscribedAt = now;
    }

    public void Activate()
    {
        Status = SubscriptionStatus.Active;
        UnsubscribedAt = null;
    }
}
