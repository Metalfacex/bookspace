using BookSpace.Core.Domain;
using BookSpace.Core.Rules;
using BookSpace.Core.Services;
using BookSpace.UnitTests.Fakes;

namespace BookSpace.UnitTests.Services;

/// <summary>US-10 შეხსენება ელფოსტით.</summary>
public class ReminderServiceTests
{
    private static readonly DateOnly Today = new(2026, 11, 10);

    private readonly InMemoryBookingRepository _bookings = new();
    private readonly InMemoryUserRepository _users = new();
    private readonly FakeEmailSender _email = new();
    private readonly FakeClock _clock = new(new DateTime(2026, 11, 10, 13, 0, 0));
    private readonly ReminderService _sut;
    private readonly User _student;
    private readonly Room _room = new() { Id = 1, Name = "ოთახი 101" };

    public ReminderServiceTests()
    {
        _student = _users.Seed("nino@uni.edu.ge");
        _sut = new ReminderService(_bookings, _users, _email, _clock, new BookingRules { ReminderMinutesBefore = 60 });
    }

    [Fact]
    public async Task SendDueReminders_BookingStartsInOneHour_SendsOneEmail()
    {
        // Arrange — ახლა 13:00, ჯავშანი 14:00
        var booking = _bookings.Seed(_student.Id, _room.Id, Today, 14, room: _room);

        // Act
        var sent = await _sut.SendDueRemindersAsync();

        // Assert
        Assert.Equal(1, sent);
        var email = Assert.Single(_email.Sent);
        Assert.Equal("nino@uni.edu.ge", email.To);
        Assert.Contains("ოთახი 101", email.Body);
        Assert.NotNull(booking.ReminderSentAt);
    }

    [Fact]
    public async Task SendDueReminders_RunTwice_SendsOnlyOnce()
    {
        // Arrange
        _bookings.Seed(_student.Id, _room.Id, Today, 14);

        // Act
        await _sut.SendDueRemindersAsync();
        var secondRun = await _sut.SendDueRemindersAsync();

        // Assert
        Assert.Equal(0, secondRun);
        Assert.Single(_email.Sent);
    }

    [Fact]
    public async Task SendDueReminders_CancelledBooking_SendsNothing()
    {
        // Arrange
        _bookings.Seed(_student.Id, _room.Id, Today, 14, BookingStatus.Cancelled);

        // Act
        var sent = await _sut.SendDueRemindersAsync();

        // Assert
        Assert.Equal(0, sent);
        Assert.Empty(_email.Sent);
    }

    [Fact]
    public async Task SendDueReminders_BookingMoreThanOneHourAway_NotYet()
    {
        // Arrange — 15:00 ჯავშანი = 2 საათი
        _bookings.Seed(_student.Id, _room.Id, Today, 15);

        // Act
        var sent = await _sut.SendDueRemindersAsync();

        // Assert
        Assert.Equal(0, sent);
    }
}
