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

public sealed class NotificationWorker(IServiceScopeFactory scopes, TimeProvider clock, ILogger<NotificationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<BrandDbContext>();
                await using var tx = await db.Database.BeginTransactionAsync(stoppingToken);
                await SqlLocks.AcquireAsync(db, "notification-dispatch", stoppingToken);
                var messages = await db.Notifications.Where(x => x.SentAt == null && x.Attempts < 10 && (x.NextAttemptAt == null || x.NextAttemptAt <= clock.GetUtcNow())).OrderBy(x => x.CreatedAt).Take(20).ToListAsync(stoppingToken);
                foreach (var message in messages)
                {
                    try
                    {
                        if (message.Channel == "telegram")
                            await scope.ServiceProvider.GetRequiredService<IAdminNotificationService>().NotifyAsync(message.Body, stoppingToken);
                        else
                            await scope.ServiceProvider.GetRequiredService<IEmailSender>().SendAsync(message.Recipient, message.Subject, message.Body, stoppingToken);
                        message.SentAt = clock.GetUtcNow();
                    }
                    catch (Exception) when (!stoppingToken.IsCancellationRequested)
                    {
                        logger.LogWarning("Notification {NotificationId} delivery failed", message.Id);
                        message.NextAttemptAt = clock.GetUtcNow().AddMinutes(Math.Min(360, Math.Pow(2, message.Attempts)));
                    }

                    message.Attempts++;
                }

                await db.SaveChangesAsync(stoppingToken);
                await tx.CommitAsync(stoppingToken);
            }
            catch (Exception) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError("Notification dispatcher could not complete this cycle");
            }
        }
    }
}
