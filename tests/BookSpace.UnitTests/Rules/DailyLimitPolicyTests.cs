using BookSpace.Core.Domain;
using BookSpace.Core.Rules;

namespace BookSpace.UnitTests.Rules;

/// <summary>
/// TDD-ით დაწერილი ფუნქციის ტესტები (Red → Green → Refactor).
/// წესი: სტუდენტს ერთ დღეში მაქსიმუმ 3 აქტიური ჯავშანი შეუძლია.
/// </summary>
public class DailyLimitPolicyTests
{
    private static readonly DateOnly Day = new(2026, 11, 11);

    private static List<Booking> ActiveBookings(int count, DateOnly date) =>
        Enumerable.Range(0, count)
            .Select(i => new Booking { Date = date, StartHour = 9 + i, Status = BookingStatus.Active })
            .ToList();

    [Fact]
    public void CanBook_NoBookingsYet_ReturnsTrue()
    {
        // Arrange
        var policy = new DailyLimitPolicy(maxPerDay: 3);

        // Act
        var result = policy.CanBook(new List<Booking>(), Day);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(2, true)]   // ლიმიტამდე ერთი — ჯერ კიდევ შეიძლება
    [InlineData(3, false)]  // ზუსტად ლიმიტზე — აღარ შეიძლება
    [InlineData(4, false)]  // ლიმიტს ზემოთ (მაგ. ძველი მონაცემები)
    public void CanBook_AtBoundaryOfLimit_ReturnsExpected(int existing, bool expected)
    {
        // Arrange
        var policy = new DailyLimitPolicy(maxPerDay: 3);
        var bookings = ActiveBookings(existing, Day);

        // Act
        var result = policy.CanBook(bookings, Day);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CanBook_CancelledBookingsAreNotCounted()
    {
        // Arrange
        var policy = new DailyLimitPolicy(maxPerDay: 3);
        var bookings = ActiveBookings(2, Day);
        bookings.Add(new Booking { Date = Day, StartHour = 15, Status = BookingStatus.Cancelled });
        bookings.Add(new Booking { Date = Day, StartHour = 16, Status = BookingStatus.Cancelled });

        // Act
        var result = policy.CanBook(bookings, Day);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanBook_BookingsOnOtherDaysAreNotCounted()
    {
        // Arrange
        var policy = new DailyLimitPolicy(maxPerDay: 3);
        var bookings = ActiveBookings(3, Day.AddDays(1));

        // Act
        var result = policy.CanBook(bookings, Day);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void RemainingFor_TwoUsed_ReturnsOne()
    {
        // Arrange
        var policy = new DailyLimitPolicy(maxPerDay: 3);

        // Act
        var remaining = policy.RemainingFor(ActiveBookings(2, Day), Day);

        // Assert
        Assert.Equal(1, remaining);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_LimitBelowOne_Throws(int invalidLimit)
    {
        // Arrange + Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new DailyLimitPolicy(invalidLimit));
    }
}
