using BookSpace.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace BookSpace.Infrastructure.Services;

/// <summary>
/// დეველოპმენტის ელფოსტის გამგზავნი: წერილს ლოგში წერს.
/// პროდაქშენში ჩანაცვლდება SMTP/SendGrid იმპლემენტაციით იგივე IEmailSender ინტერფეისით.
/// </summary>
public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("EMAIL to {To}: {Subject} — {Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
