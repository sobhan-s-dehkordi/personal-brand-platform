namespace PersonalBrand.Core;

public interface IEmailSender
{
    Task SendAsync(string recipient, string subject, string body, CancellationToken ct);
}
