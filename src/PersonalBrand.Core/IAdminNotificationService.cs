namespace PersonalBrand.Core;

public interface IAdminNotificationService
{
    Task NotifyAsync(string message, CancellationToken ct);
}
