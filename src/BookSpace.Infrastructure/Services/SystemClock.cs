using BookSpace.Core.Abstractions;

namespace BookSpace.Infrastructure.Services;

/// <summary>აბრუნებს მიმდინარე დროს უნივერსიტეტის დროის სარტყელში (ნაგულისხმევად Asia/Tbilisi).</summary>
public class SystemClock : IClock
{
    private readonly TimeZoneInfo _zone;

    public SystemClock(string? timeZoneId)
    {
        _zone = TryFindZone(timeZoneId) ?? TimeZoneInfo.Local;
    }

    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _zone);

    private static TimeZoneInfo? TryFindZone(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return null;
        }
    }
}
