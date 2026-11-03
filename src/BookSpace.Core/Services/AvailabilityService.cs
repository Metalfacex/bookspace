using BookSpace.Core.Abstractions;
using BookSpace.Core.Common;
using BookSpace.Core.Rules;

namespace BookSpace.Core.Services;

/// <summary>US-04 თავისუფალი დროების ნახვა.</summary>
public class AvailabilityService
{
    private readonly IRoomRepository _rooms;
    private readonly IBookingRepository _bookings;
    private readonly IClock _clock;
    private readonly BookingRules _rules;

    public AvailabilityService(IRoomRepository rooms, IBookingRepository bookings, IClock clock, BookingRules rules)
    {
        _rooms = rooms;
        _bookings = bookings;
        _clock = clock;
        _rules = rules;
    }

    public async Task<Result<DaySchedule>> GetScheduleAsync(int roomId, DateOnly date, CancellationToken ct = default)
    {
        var room = await _rooms.GetByIdAsync(roomId, ct);
        if (room is null || !room.IsActive)
        {
            return Result<DaySchedule>.Fail(ErrorCode.NotFound, "ოთახი ვერ მოიძებნა.");
        }

        var booked = (await _bookings.GetActiveForRoomAsync(roomId, date, ct))
            .Select(b => b.StartHour)
            .ToHashSet();

        var now = _clock.Now;
        var slots = new List<TimeSlot>();
        for (var hour = _rules.OpeningHour; hour < _rules.ClosingHour; hour++)
        {
            var startsAt = date.ToDateTime(new TimeOnly(hour, 0));
            var state = booked.Contains(hour)
                ? SlotState.Booked
                : startsAt <= now ? SlotState.Past : SlotState.Free;
            slots.Add(new TimeSlot(hour, state));
        }

        return Result<DaySchedule>.Ok(new DaySchedule(roomId, date, slots));
    }
}
