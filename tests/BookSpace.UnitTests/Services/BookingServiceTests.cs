using BookSpace.Core.Common;
using BookSpace.Core.Domain;
using BookSpace.Core.Rules;
using BookSpace.Core.Services;
using BookSpace.UnitTests.Fakes;

namespace BookSpace.UnitTests.Services;

/// <summary>US-05 დაჯავშნა, US-06 ჩემი ჯავშნები, US-07 გაუქმება.</summary>
public class BookingServiceTests
{
    private const int StudentId = 1;
    private const int OtherStudentId = 2;
    private static readonly DateOnly Today = new(2026, 11, 10);      // "ახლა" = 10:30
    private static readonly DateOnly Tomorrow = Today.AddDays(1);

    private readonly InMemoryRoomRepository _rooms = new();
    private readonly InMemoryBookingRepository _bookings = new();
    private readonly FakeClock _clock = new();
    private readonly BookingService _sut;
    private readonly Room _room;

    public BookingServiceTests()
    {
        _sut = new BookingService(_rooms, _bookings, _clock, new BookingRules
        {
            OpeningHour = 8,
            ClosingHour = 22,
            MaxBookingsPerDay = 3,
            MaxDaysAhead = 14
        });
        _room = _rooms.Seed();
    }

    // ---------- US-05 ----------

    [Fact]
    public async Task Book_FreeSlot_SavesBooking()
    {
        // Arrange — იხ. კონსტრუქტორი (ოთახი არსებობს, სლოტი თავისუფალია)

        // Act
        var result = await _sut.BookAsync(StudentId, _room.Id, Tomorrow, 10);

        // Assert
        Assert.True(result.IsSuccess);
        var saved = Assert.Single(_bookings.Items);
        Assert.Equal(StudentId, saved.UserId);
        Assert.Equal(BookingStatus.Active, saved.Status);
    }

    [Theory]
    [InlineData(7, false)]
    [InlineData(8, true)]
    [InlineData(21, true)]
    [InlineData(22, false)]
    public async Task Book_OpeningHoursBoundaries(int hour, bool expectedSuccess)
    {
        // Act
        var result = await _sut.BookAsync(StudentId, _room.Id, Tomorrow, hour);

        // Assert
        Assert.Equal(expectedSuccess, result.IsSuccess);
    }

    [Theory]
    [InlineData(10, false)] // 10:00 უკვე დაიწყო (ახლა 10:30)
    [InlineData(11, true)]  // მომდევნო სლოტი
    public async Task Book_Today_PastSlotIsRejected(int hour, bool expectedSuccess)
    {
        // Act
        var result = await _sut.BookAsync(StudentId, _room.Id, Today, hour);

        // Assert
        Assert.Equal(expectedSuccess, result.IsSuccess);
    }

    [Theory]
    [InlineData(14, true)]
    [InlineData(15, false)]
    public async Task Book_MaxDaysAheadBoundary(int daysAhead, bool expectedSuccess)
    {
        // Act
        var result = await _sut.BookAsync(StudentId, _room.Id, Today.AddDays(daysAhead), 10);

        // Assert
        Assert.Equal(expectedSuccess, result.IsSuccess);
    }

    [Fact]
    public async Task Book_SlotAlreadyTaken_ReturnsConflict()
    {
        // Arrange
        _bookings.Seed(OtherStudentId, _room.Id, Tomorrow, 10);

        // Act
        var result = await _sut.BookAsync(StudentId, _room.Id, Tomorrow, 10);

        // Assert
        Assert.Equal(ErrorCode.Conflict, result.Error);
        Assert.Single(_bookings.Items);
    }

    [Fact]
    public async Task Book_ConcurrentRequestWinsAtDatabase_ReturnsConflict()
    {
        // Arrange — წინასწარი შემოწმება გადის, მაგრამ ბაზის unique index უარყოფს
        _bookings.SimulateConcurrentInsert = true;

        // Act
        var result = await _sut.BookAsync(StudentId, _room.Id, Tomorrow, 10);

        // Assert
        Assert.Equal(ErrorCode.Conflict, result.Error);
    }

    [Fact]
    public async Task Book_SameHourInAnotherRoom_ReturnsConflict()
    {
        // Arrange
        var otherRoom = _rooms.Seed("ოთახი 204");
        _bookings.Seed(StudentId, otherRoom.Id, Tomorrow, 10);

        // Act
        var result = await _sut.BookAsync(StudentId, _room.Id, Tomorrow, 10);

        // Assert
        Assert.Equal(ErrorCode.Conflict, result.Error);
    }

    [Fact]
    public async Task Book_InactiveRoom_ReturnsNotFound()
    {
        // Arrange
        var closed = _rooms.Seed("დახურული", isActive: false);

        // Act
        var result = await _sut.BookAsync(StudentId, closed.Id, Tomorrow, 10);

        // Assert
        Assert.Equal(ErrorCode.NotFound, result.Error);
    }

    // ---------- US-06 ----------

    [Fact]
    public async Task GetMyBookings_ReturnsOnlyOwnBookings_SplitIntoUpcomingAndHistory()
    {
        // Arrange
        _bookings.Seed(StudentId, _room.Id, Tomorrow, 10);
        _bookings.Seed(StudentId, _room.Id, Today.AddDays(-1), 10);
        _bookings.Seed(StudentId, _room.Id, Tomorrow, 12, BookingStatus.Cancelled);
        _bookings.Seed(OtherStudentId, _room.Id, Tomorrow, 11);

        // Act
        var my = await _sut.GetMyBookingsAsync(StudentId);

        // Assert
        Assert.Single(my.Upcoming);
        Assert.Equal(2, my.History.Count);
        Assert.All(my.Upcoming.Concat(my.History), b => Assert.Equal(StudentId, b.UserId));
    }

    [Fact]
    public async Task GetMyBookings_NoBookings_IsEmpty()
    {
        // Act
        var my = await _sut.GetMyBookingsAsync(StudentId);

        // Assert
        Assert.True(my.IsEmpty);
    }

    // ---------- US-07 ----------

    [Fact]
    public async Task Cancel_OwnFutureBooking_FreesTheSlot()
    {
        // Arrange
        var booking = _bookings.Seed(StudentId, _room.Id, Tomorrow, 10);

        // Act
        var cancel = await _sut.CancelAsync(StudentId, booking.Id);
        var rebook = await _sut.BookAsync(OtherStudentId, _room.Id, Tomorrow, 10);

        // Assert
        Assert.True(cancel.IsSuccess);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.True(rebook.IsSuccess);
    }

    [Fact]
    public async Task Cancel_SomeoneElsesBooking_IsForbidden()
    {
        // Arrange
        var booking = _bookings.Seed(OtherStudentId, _room.Id, Tomorrow, 10);

        // Act
        var result = await _sut.CancelAsync(StudentId, booking.Id);

        // Assert
        Assert.Equal(ErrorCode.Forbidden, result.Error);
        Assert.Equal(BookingStatus.Active, booking.Status);
    }

    [Fact]
    public async Task Cancel_AlreadyStartedBooking_IsRejected()
    {
        // Arrange — 10:00 სლოტი დაიწყო (ახლა 10:30)
        var booking = _bookings.Seed(StudentId, _room.Id, Today, 10);

        // Act
        var result = await _sut.CancelAsync(StudentId, booking.Id);

        // Assert
        Assert.Equal(ErrorCode.Validation, result.Error);
    }

    [Fact]
    public async Task Cancel_AlreadyCancelled_ReturnsConflict()
    {
        // Arrange
        var booking = _bookings.Seed(StudentId, _room.Id, Tomorrow, 10, BookingStatus.Cancelled);

        // Act
        var result = await _sut.CancelAsync(StudentId, booking.Id);

        // Assert
        Assert.Equal(ErrorCode.Conflict, result.Error);
    }

    [Fact]
    public async Task Cancel_UnknownBooking_ReturnsNotFound()
    {
        // Act
        var result = await _sut.CancelAsync(StudentId, 999);

        // Assert
        Assert.Equal(ErrorCode.NotFound, result.Error);
    }
}
