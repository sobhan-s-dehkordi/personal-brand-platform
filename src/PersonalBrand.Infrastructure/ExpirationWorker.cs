using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PersonalBrand.Core;

namespace PersonalBrand.Infrastructure;

public sealed class ExpirationWorker(IServiceScopeFactory scopes, TimeProvider clock, ILogger<ExpirationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(ct))
        {
            try
            {
                using var scope = scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<BrandDbContext>();
                var ids = await db.Reservations.AsNoTracking().Where(x => x.Status == ReservationStatus.PendingPayment && x.HoldExpiresAt <= clock.GetUtcNow()).Select(x => x.WorkshopId).Distinct().OrderBy(x => x).Take(50).ToListAsync(ct);
                foreach (var id in ids)
                {
                    await using var tx = await db.Database.BeginTransactionAsync(ct);
                    await db.Workshops.FromSqlInterpolated($"SELECT * FROM Workshops WITH (UPDLOCK,HOLDLOCK) WHERE Id={id}").LoadAsync(ct);
                    var expired = await db.Reservations.Where(x => x.WorkshopId == id && x.Status == ReservationStatus.PendingPayment && x.HoldExpiresAt <= clock.GetUtcNow()).OrderBy(x => x.HoldExpiresAt).Take(100).ToListAsync(ct);
                    foreach (var r in expired)
                        r.Expire(clock.GetUtcNow());
                    await db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                    db.ChangeTracker.Clear();
                }
            }
            catch (Exception) when (!ct.IsCancellationRequested)
            {
                logger.LogWarning("Expiration cleanup failed; availability still excludes expired holds");
            }
        }
    }
}
