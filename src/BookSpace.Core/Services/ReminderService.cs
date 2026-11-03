using BookSpace.Core.Abstractions;
using BookSpace.Core.Rules;

namespace BookSpace.Core.Services;

/// <summary>US-10 შეხსენება ელფოსტით (ჯავშნამდე 1 საათით ადრე).</summary>
public class ReminderService
{
    private readonly IBookingRepository _bookings;
    private readonly IUserRepository _users;
    private readonly IEmailSender _email;
    private readonly IClock _clock;
    private readonly BookingRules _rules;

    public ReminderService(IBookingRepository bookings, IUserRepository users, IEmailSender email, IClock clock,
        BookingRules rules)
    {
        _bookings = bookings;
        _users = users;
        _email = email;
        _clock = clock;
        _rules = rules;
    }

    /// <summary>აგზავნის შეხსენებებს იმ ჯავშნებზე, რომლებიც მომდევნო N წუთში იწყება. აბრუნებს გაგზავნილების რაოდენობას.</summary>
    public async Task<int> SendDueRemindersAsync(CancellationToken ct = default)
    {
        var now = _clock.Now;
        var windowEnd = now.AddMinutes(_rules.ReminderMinutesBefore);
        var today = DateOnly.FromDateTime(now);

        var candidates = await _bookings.GetActiveInRangeAsync(today, DateOnly.FromDateTime(windowEnd), ct);
        var due = candidates
            .Where(b => b.IsActive && b.ReminderSentAt is null && b.StartsAt > now && b.StartsAt <= windowEnd)
            .ToList();

        var sent = 0;
        foreach (var booking in due)
        {
            var user = booking.User ?? await _users.GetByIdAsync(booking.UserId, ct);
            if (user is null)
            {
                continue;
            }

            var roomName = booking.Room?.Name ?? $"#{booking.RoomId}";
            await _email.SendAsync(
                user.Email,
                "BookSpace — შეხსენება ჯავშანზე",
                $"გახსენებთ: ოთახი {roomName}, {booking.Date:dd.MM.yyyy} {booking.StartHour:00}:00.",
                ct);

            booking.ReminderSentAt = now;
            await _bookings.UpdateAsync(booking, ct);
            sent++;
        }

        return sent;
    }
}
