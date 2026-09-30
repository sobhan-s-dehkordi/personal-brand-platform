using System.Net;
using System.Net.Mail;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PersonalBrand.Core;
using Ganss.Xss;
using Markdig;

namespace PersonalBrand.Infrastructure;

public sealed class TelegramNotificationService(HttpClient client, IOptions<TelegramOptions> options) : IAdminNotificationService
{
    public async Task NotifyAsync(string message, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(options.Value.BotToken))
            throw new InvalidOperationException("Telegram is not configured.");
        using var response = await client.PostAsJsonAsync($"https://api.telegram.org/bot{options.Value.BotToken}/sendMessage", new
        {
            chat_id = options.Value.ChatId,
            text = message
        }, ct);
        response.EnsureSuccessStatusCode();
    }
}
