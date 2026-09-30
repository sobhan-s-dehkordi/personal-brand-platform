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

public sealed class SmtpEmailSender(IOptions<EmailOptions> options) : IEmailSender
{
    public async Task SendAsync(string recipient, string subject, string body, CancellationToken ct)
    {
        var o = options.Value;
        if (string.IsNullOrEmpty(o.Host))
            throw new InvalidOperationException("SMTP is not configured.");
        using var client = new SmtpClient(o.Host, o.Port)
        {
            EnableSsl = o.EnableSsl,
            Credentials = new NetworkCredential(o.Username, o.Password)
        };
        using var message = new MailMessage(o.From, recipient, subject, body);
        await client.SendMailAsync(message, ct);
    }
}
