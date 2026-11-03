using BookSpace.Core.Common;
using BookSpace.Core.Rules;
using BookSpace.Core.Services;
using BookSpace.UnitTests.Fakes;

namespace BookSpace.UnitTests.Services;

/// <summary>US-04 თავისუფალი დროების ნახვა.</summary>
public class AvailabilityServiceTests
{
    private static readonly DateOnly Tomorrow = new(2026, 11, 11);
    private static readonly DateOnly Today = new(2026, 11, 10);

    private readonly InMemoryRoomRepository _rooms = new();
    private readonly InMemoryBookingRepository _bookings = new();
    private readonly AvailabilityService _sut;

    public AvailabilityServiceTests()
    {
        _sut = new AvailabilityService(_rooms, _bookings, new FakeClock(), new BookingRules());
    }

    [Fact]
    public async Task GetSchedule_ShowsBookedSlotsAsBooked()
    {
        // Arrange
        var room = _rooms.Seed();
        _bookings.Seed(userId: 1, room.Id, Tomorrow, 10);
        _bookings.Seed(userId: 2, room.Id, Tomorrow, 14);

        // Act
        var result = await _sut.GetScheduleAsync(room.Id, Tomorrow);

        // Assert
        var slots = result.Value!.Slots;
        Assert.Equal(14, slots.Count); // 08:00–22:00
        Assert.Equal(SlotState.Booked, slots.Single(s => s.StartHour == 10).State);
        Assert.Equal(SlotState.Booked, slots.Single(s => s.StartHour == 14).State);
        Assert.Equal(12, slots.Count(s => s.IsFree));
    }

    [Fact]
    public async Task GetSchedule_AllSlotsBooked_HasNoFreeSlots()
    {
        // Arrange
        var room = _rooms.Seed();
        for (var hour = 8; hour < 22; hour++)
        {
            _bookings.Seed(userId: hour, room.Id, Tomorrow, hour);
        }

        // Act
        var result = await _sut.GetScheduleAsync(room.Id, Tomorrow);

        // Assert
        Assert.False(result.Value!.HasFreeSlots);
    }

    [Fact]
    public async Task GetSchedule_Today_SlotsThatAlreadyStartedArePast()
    {
        // Arrange — "ახლა" არის 10:30
        var room = _rooms.Seed();

        // Act
        var slots = (await _sut.GetScheduleAsync(room.Id, Today)).Value!.Slots;

        // Assert
        Assert.Equal(SlotState.Past, slots.Single(s => s.StartHour == 10).State);
        Assert.Equal(SlotState.Free, slots.Single(s => s.StartHour == 11).State);
    }

    [Fact]
    public async Task GetSchedule_CancelledBookingDoesNotBlockSlot()
    {
        // Arrange
        var room = _rooms.Seed();
        _bookings.Seed(1, room.Id, Tomorrow, 9, BookSpace.Core.Domain.BookingStatus.Cancelled);

        // Act
        var slots = (await _sut.GetScheduleAsync(room.Id, Tomorrow)).Value!.Slots;

        // Assert
        Assert.True(slots.Single(s => s.StartHour == 9).IsFree);
    }

    [Fact]
    public async Task GetSchedule_UnknownRoom_ReturnsNotFound()
    {
        // Act
        var result = await _sut.GetScheduleAsync(42, Tomorrow);

        // Assert
        Assert.Equal(ErrorCode.NotFound, result.Error);
    }
}
