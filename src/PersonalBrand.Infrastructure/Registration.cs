using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalBrand.Core;

namespace PersonalBrand.Infrastructure;

public static class Registration
{
    public static IServiceCollection AddBrand(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BrandDbContext>(o => o.UseSqlServer(configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection for SQL Server.")));
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<SafeMarkdown>();
        services.AddScoped<SiteContent>();
        services.Configure<SiteOptions>(configuration.GetSection("Site"));
        services.AddOptions<WorkshopReservationOptions>().Bind(configuration.GetSection("WorkshopReservation")).Validate(x => x.HoldDurationMinutes is > 0 and <= 1440).ValidateOnStart();
        services.AddOptions<StorageOptions>().Bind(configuration.GetSection("Storage")).PostConfigure<Microsoft.Extensions.Hosting.IHostEnvironment>((o, env) => o.Root = Path.GetFullPath(o.Root, env.ContentRootPath));
        services.Configure<EmailOptions>(configuration.GetSection("Email"));
        services.Configure<TelegramOptions>(configuration.GetSection("Telegram"));
        services.AddScoped<AudienceService>();
        services.AddScoped<IAudienceService>(sp => sp.GetRequiredService<AudienceService>());
        services.AddScoped<IReservationService, ReservationService>();
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddHttpClient<IAdminNotificationService, TelegramNotificationService>().RemoveAllLoggers();
        return services;
    }
}
