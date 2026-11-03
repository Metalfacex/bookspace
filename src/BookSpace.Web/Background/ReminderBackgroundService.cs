using BookSpace.Core.Services;

namespace BookSpace.Web.Background;

/// <summary>ყოველ 5 წუთში ერთხელ უშვებს ReminderService-ს (US-10).</summary>
public class ReminderBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReminderBackgroundService> _logger;

    public ReminderBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var reminders = scope.ServiceProvider.GetRequiredService<ReminderService>();
                var sent = await reminders.SendDueRemindersAsync(stoppingToken);
                if (sent > 0)
                {
                    _logger.LogInformation("გაიგზავნა {Count} შეხსენება.", sent);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "შეხსენებების გაგზავნა ვერ მოხერხდა.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
