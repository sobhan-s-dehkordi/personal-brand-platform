using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PersonalBrand.Infrastructure;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBrand(builder.Configuration);
builder.Services.AddAntiforgery(o =>
{
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.HttpOnly = true;
});
var protection = builder.Services.AddDataProtection().SetApplicationName("PersonalBrand.Admin");
if (builder.Configuration["Security:DataProtectionPath"] is string keyPath)
    protection.PersistKeysToFileSystem(new DirectoryInfo(keyPath));
builder.Services.AddIdentity<IdentityUser, IdentityRole>(o =>
{
    o.Password.RequiredLength = 12;
    o.Lockout.MaxFailedAccessAttempts = 5;
    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    o.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<BrandDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(o =>
{
    o.Cookie.Name = "__Host-PersonalBrand.Admin";
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Strict;
    o.LoginPath = "/Account/Login";
    o.AccessDeniedPath = "/Account/Denied";
    o.ExpireTimeSpan = TimeSpan.FromHours(4);
});
builder.Services.AddAuthorization(o => o.AddPolicy("Admin", p => p.RequireRole("Administrator")));
builder.Services.AddRazorPages(o =>
{
    o.Conventions.AddPageRoute("/Content/Edit", "/{kind:regex(^(Articles|Courses|Projects|Workshops)$)}/Create");
    o.Conventions.AuthorizeFolder("/", "Admin");
    o.Conventions.AllowAnonymousToPage("/Account/Login");
    o.Conventions.AllowAnonymousToPage("/Error");
    o.Conventions.AllowAnonymousToPage("/Account/Denied");
});
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = 429;
    o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(c => RateLimitPartition.GetFixedWindowLimiter(c.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1) }));
});
if (!args.Contains("--migrate") && !args.Contains("--seed") && !args.Contains("--create-admin"))
{
    builder.Services.AddHostedService<NotificationWorker>();
    builder.Services.AddHostedService<ExpirationWorker>();
}

builder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(o => o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
var app = builder.Build();
if (args.Contains("--migrate") || args.Contains("--seed") || args.Contains("--create-admin"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BrandDbContext>();
    if (args.Contains("--migrate"))
        await db.Database.MigrateAsync();
    if (args.Contains("--seed"))
    {
        if (!app.Environment.IsDevelopment())
            throw new InvalidOperationException("Sample seed is development only.");
        await DevelopmentSeed.RunAsync(db);
    }

    if (args.Contains("--create-admin"))
        await DevelopmentSeed.CreateAdminAsync(scope.ServiceProvider, builder.Configuration);
    return;
}

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<SiteContent>().EnsureDefaultsAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.Use(async (c, next) =>
{
    c.Response.Headers.CacheControl = "no-store";
    c.Response.Headers["X-Content-Type-Options"] = "nosniff";
    c.Response.Headers["Referrer-Policy"] = "no-referrer";
    c.Response.Headers["Content-Security-Policy"] = "default-src 'self'; img-src 'self' data:; style-src 'self'; script-src 'self'; object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'";
    await next();
});
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
public partial class AdminProgram
{
}
