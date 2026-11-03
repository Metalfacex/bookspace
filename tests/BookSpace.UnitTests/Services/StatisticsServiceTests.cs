using BookSpace.Core.Common;
using BookSpace.Core.Domain;
using BookSpace.Core.Services;
using BookSpace.UnitTests.Fakes;

namespace BookSpace.UnitTests.Services;

/// <summary>US-11 ოთახების გამოყენების სტატისტიკა.</summary>
public class StatisticsServiceTests
{
    private static readonly DateOnly From = new(2026, 11, 1);
    private static readonly DateOnly To = new(2026, 11, 30);

    private readonly InMemoryRoomRepository _rooms = new();
    private readonly InMemoryBookingRepository _bookings = new();
    private readonly StatisticsService _sut;

    public StatisticsServiceTests()
    {
        _sut = new StatisticsService(_rooms, _bookings);
    }

    [Fact]
    public async Task GetRoomUsage_CountsActiveBookingsPerRoomInPeriod()
    {
        // Arrange
        var a = _rooms.Seed("A");
        var b = _rooms.Seed("B");
        _bookings.Seed(1, a.Id, new DateOnly(2026, 11, 5), 10);
        _bookings.Seed(2, a.Id, new DateOnly(2026, 11, 6), 10);
        _bookings.Seed(1, b.Id, new DateOnly(2026, 11, 7), 10);
        _bookings.Seed(1, b.Id, new DateOnly(2026, 11, 8), 10, BookingStatus.Cancelled); // არ ითვლება
        _bookings.Seed(1, b.Id, new DateOnly(2026, 12, 1), 10);                          // პერიოდს გარეთ

        // Act
        var usage = (await _sut.GetRoomUsageAsync(From, To)).Value!;

        // Assert
        Assert.Equal(2, usage.Single(u => u.RoomName == "A").BookingCount);
        Assert.Equal(1, usage.Single(u => u.RoomName == "B").BookingCount);
    }

    [Fact]
    public async Task GetRoomUsage_NoBookings_ReturnsZeroForEveryRoom()
    {
        // Arrange
        _rooms.Seed("A");
        _rooms.Seed("B");

        // Act
        var usage = (await _sut.GetRoomUsageAsync(From, To)).Value!;

        // Assert
        Assert.Equal(2, usage.Count);
        Assert.All(usage, u => Assert.Equal(0, u.BookingCount));
    }

    [Fact]
    public async Task GetRoomUsage_FromAfterTo_ReturnsValidationError()
    {
        // Act
        var result = await _sut.GetRoomUsageAsync(To, From);

        // Assert
        Assert.Equal(ErrorCode.Validation, result.Error);
    }

    [Theory]
    [InlineData(365, true)]  // 366 დღე ჩათვლით — ზღვარზე
    [InlineData(366, false)] // 367 დღე — ზღვარს გარეთ
    public async Task GetRoomUsage_PeriodLengthBoundary(int extraDays, bool expectedSuccess)
    {
        // Act
        var result = await _sut.GetRoomUsageAsync(From, From.AddDays(extraDays));

        // Assert
        Assert.Equal(expectedSuccess, result.IsSuccess);
    }
}
