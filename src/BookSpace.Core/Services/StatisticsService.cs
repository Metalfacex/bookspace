using BookSpace.Core.Abstractions;
using BookSpace.Core.Common;

namespace BookSpace.Core.Services;

public record RoomUsage(int RoomId, string RoomName, int BookingCount);

/// <summary>US-11 ოთახების გამოყენების სტატისტიკა.</summary>
public class StatisticsService
{
    public const int MaxPeriodDays = 366;

    private readonly IRoomRepository _rooms;
    private readonly IBookingRepository _bookings;

    public StatisticsService(IRoomRepository rooms, IBookingRepository bookings)
    {
        _rooms = rooms;
        _bookings = bookings;
    }

    public async Task<Result<IReadOnlyList<RoomUsage>>> GetRoomUsageAsync(DateOnly from, DateOnly to,
        CancellationToken ct = default)
    {
        if (from > to)
        {
            return Result<IReadOnlyList<RoomUsage>>.Fail(ErrorCode.Validation, "საწყისი თარიღი საბოლოოზე გვიან არის.");
        }

        if (to.DayNumber - from.DayNumber + 1 > MaxPeriodDays)
        {
            return Result<IReadOnlyList<RoomUsage>>.Fail(ErrorCode.Validation,
                $"პერიოდი მაქსიმუმ {MaxPeriodDays} დღეა.");
        }

        var rooms = await _rooms.GetAllAsync(includeInactive: true, ct);
        var counts = (await _bookings.GetActiveInRangeAsync(from, to, ct))
            .GroupBy(b => b.RoomId)
            .ToDictionary(g => g.Key, g => g.Count());

        IReadOnlyList<RoomUsage> usage = rooms
            .Select(r => new RoomUsage(r.Id, r.Name, counts.GetValueOrDefault(r.Id)))
            .OrderByDescending(u => u.BookingCount)
            .ThenBy(u => u.RoomName)
            .ToList();

        return Result<IReadOnlyList<RoomUsage>>.Ok(usage);
    }
}
