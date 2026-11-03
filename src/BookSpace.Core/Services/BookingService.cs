using BookSpace.Core.Abstractions;
using BookSpace.Core.Common;
using BookSpace.Core.Domain;
using BookSpace.Core.Rules;

namespace BookSpace.Core.Services;

/// <summary>US-05 დაჯავშნა, US-06 ჩემი ჯავშნები, US-07 გაუქმება.</summary>
public class BookingService
{
    private readonly IRoomRepository _rooms;
    private readonly IBookingRepository _bookings;
    private readonly IClock _clock;
    private readonly BookingRules _rules;

    public BookingService(IRoomRepository rooms, IBookingRepository bookings, IClock clock, BookingRules rules)
    {
        _rooms = rooms;
        _bookings = bookings;
        _clock = clock;
        _rules = rules;
    }

    public async Task<Result<Booking>> BookAsync(int userId, int roomId, DateOnly date, int startHour,
        CancellationToken ct = default)
    {
        if (!_rules.IsWithinOpeningHours(startHour))
        {
            return Result<Booking>.Fail(ErrorCode.Validation,
                $"დაჯავშნა შესაძლებელია {_rules.OpeningHour:00}:00-დან {_rules.ClosingHour:00}:00-მდე.");
        }

        var now = _clock.Now;
        var today = DateOnly.FromDateTime(now);
        var startsAt = date.ToDateTime(new TimeOnly(startHour, 0));

        if (startsAt <= now)
        {
            return Result<Booking>.Fail(ErrorCode.Validation, "წარსული დროის დაჯავშნა შეუძლებელია.");
        }

        if (date > today.AddDays(_rules.MaxDaysAhead))
        {
            return Result<Booking>.Fail(ErrorCode.Validation,
                $"დაჯავშნა შესაძლებელია მაქსიმუმ {_rules.MaxDaysAhead} დღით ადრე.");
        }

        var room = await _rooms.GetByIdAsync(roomId, ct);
        if (room is null || !room.IsActive)
        {
            return Result<Booking>.Fail(ErrorCode.NotFound, "ოთახი ვერ მოიძებნა.");
        }

        var roomBookings = await _bookings.GetActiveForRoomAsync(roomId, date, ct);
        if (roomBookings.Any(b => b.StartHour == startHour))
        {
            return Result<Booking>.Fail(ErrorCode.Conflict, "ეს დრო უკვე დაკავებულია.");
        }

        var userBookings = await _bookings.GetForUserAsync(userId, ct);
        if (userBookings.Any(b => b.IsActive && b.Date == date && b.StartHour == startHour))
        {
            return Result<Booking>.Fail(ErrorCode.Conflict, "ამ საათზე უკვე გაქვთ სხვა ჯავშანი.");
        }

        var booking = new Booking
        {
            RoomId = roomId,
            UserId = userId,
            Date = date,
            StartHour = startHour,
            Status = BookingStatus.Active,
            CreatedAt = now
        };

        // ბოლო დაცვა ერთდროული მოთხოვნებისგან — ბაზის unique index.
        if (!await _bookings.TryAddAsync(booking, ct))
        {
            return Result<Booking>.Fail(ErrorCode.Conflict, "ეს დრო უკვე დაკავებულია.");
        }

        booking.Room ??= room;
        return Result<Booking>.Ok(booking);
    }

    public async Task<MyBookings> GetMyBookingsAsync(int userId, CancellationToken ct = default)
    {
        var now = _clock.Now;
        var all = await _bookings.GetForUserAsync(userId, ct);

        var upcoming = all
            .Where(b => b.IsActive && b.EndsAt > now)
            .OrderBy(b => b.StartsAt)
            .ToList();

        var history = all
            .Except(upcoming)
            .OrderByDescending(b => b.StartsAt)
            .ToList();

        return new MyBookings(upcoming, history);
    }

    public async Task<Result> CancelAsync(int userId, int bookingId, CancellationToken ct = default)
    {
        var booking = await _bookings.GetByIdAsync(bookingId, ct);
        if (booking is null)
        {
            return Result.Fail(ErrorCode.NotFound, "ჯავშანი ვერ მოიძებნა.");
        }

        if (booking.UserId != userId)
        {
            return Result.Fail(ErrorCode.Forbidden, "სხვისი ჯავშნის გაუქმება არ შეიძლება.");
        }

        if (!booking.IsActive)
        {
            return Result.Fail(ErrorCode.Conflict, "ჯავშანი უკვე გაუქმებულია.");
        }

        if (booking.StartsAt <= _clock.Now)
        {
            return Result.Fail(ErrorCode.Validation, "დაწყებული ან წარსული ჯავშნის გაუქმება შეუძლებელია.");
        }

        booking.Status = BookingStatus.Cancelled;
        await _bookings.UpdateAsync(booking, ct);
        return Result.Ok();
    }
}

public record MyBookings(IReadOnlyList<Booking> Upcoming, IReadOnlyList<Booking> History)
{
    public bool IsEmpty => Upcoming.Count == 0 && History.Count == 0;
}
