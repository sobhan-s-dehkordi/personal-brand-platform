namespace PersonalBrand.Core;

public sealed class Notification : Entity
{
    public string Channel { get; set; } = "email";
    public string Recipient { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTimeOffset CreatedAt
    {
        get; set;
    }
    public DateTimeOffset? SentAt
    {
        get; set;
    }
    public int Attempts
    {
        get; set;
    }
    public DateTimeOffset? NextAttemptAt
    {
        get; set;
    }
}
