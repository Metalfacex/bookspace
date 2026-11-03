using BookSpace.Core.Common;
using BookSpace.Core.Domain;
using BookSpace.Core.Rules;
using BookSpace.Core.Services;
using BookSpace.Infrastructure.Data;
using BookSpace.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BookSpace.IntegrationTests;

/// <summary>
/// Integration ტესტები: დომენის სერვისი + EF Core რეპოზიტორიები + რეალური ბაზა
/// (CI-ში MS SQL Server 2022, ლოკალურად SQLite).
/// </summary>
public class BookingPersistenceTests : IAsyncLifetime
{
    private static readonly DateTime Now = new(2026, 11, 10, 10, 30, 0);
    private static readonly DateOnly Tomorrow = new(2026, 11, 11);

    private readonly TestDatabase _database = new();
    private int _roomId;
    private int _studentA;
    private int _studentB;

    public async Task InitializeAsync()
    {
        await _database.InitializeAsync();

        await using var db = _database.CreateContext();
        var room = new Room { Name = "ოთახი 101", Location = "I კორპუსი", Capacity = 6 };
        var a = new User { Email = "a@uni.edu.ge", PasswordHash = "x", CreatedAt = Now };
        var b = new User { Email = "b@uni.edu.ge", PasswordHash = "x", CreatedAt = Now };
        db.AddRange(room, a, b);
        await db.SaveChangesAsync();

        _roomId = room.Id;
        _studentA = a.Id;
        _studentB = b.Id;
    }

    public Task DisposeAsync() => _database.DisposeAsync();

    private static BookingService CreateBookingService(BookSpaceDbContext db) =>
        new(new RoomRepository(db), new BookingRepository(db), new FixedClock(Now), new BookingRules());

    [Fact]
    public async Task Book_PersistsBookingToDatabase()
    {
        // Arrange
        await using var writeDb = _database.CreateContext();
        var service = CreateBookingService(writeDb);

        // Act
        var result = await service.BookAsync(_studentA, _roomId, Tomorrow, 10);

        // Assert — ვკითხულობთ ახალი კონტექსტით, ანუ ნამდვილად ბაზიდან
        Assert.True(result.IsSuccess);
        await using var readDb = _database.CreateContext();
        var saved = await readDb.Bookings.Include(b => b.Room).SingleAsync();
        Assert.Equal(_studentA, saved.UserId);
        Assert.Equal(Tomorrow, saved.Date);
        Assert.Equal(10, saved.StartHour);
        Assert.Equal("ოთახი 101", saved.Room!.Name);
    }

    [Fact]
    public async Task UniqueIndex_RejectsSecondActiveBookingForSameSlot()
    {
        // Arrange
        await using var db = _database.CreateContext();
        var repo = new BookingRepository(db);
        await repo.TryAddAsync(new Booking { RoomId = _roomId, UserId = _studentA, Date = Tomorrow, StartHour = 10, CreatedAt = Now });

        // Act — სერვისის შემოწმებას გვერდს ვუვლით და პირდაპირ ვწერთ ბაზაში
        var second = await repo.TryAddAsync(new Booking { RoomId = _roomId, UserId = _studentB, Date = Tomorrow, StartHour = 10, CreatedAt = Now });

        // Assert
        Assert.False(second);
        await using var readDb = _database.CreateContext();
        Assert.Equal(1, await readDb.Bookings.CountAsync());
    }

    [Fact]
    public async Task CancelledSlot_CanBeBookedAgain()
    {
        // Arrange
        await using var db = _database.CreateContext();
        var service = CreateBookingService(db);
        var first = await service.BookAsync(_studentA, _roomId, Tomorrow, 10);
        await service.CancelAsync(_studentA, first.Value!.Id);

        // Act
        var second = await service.BookAsync(_studentB, _roomId, Tomorrow, 10);

        // Assert
        Assert.True(second.IsSuccess);
        await using var readDb = _database.CreateContext();
        var rows = await readDb.Bookings.OrderBy(b => b.Id).ToListAsync();
        Assert.Equal(2, rows.Count);
        Assert.Equal(BookingStatus.Cancelled, rows[0].Status);
        Assert.Equal(BookingStatus.Active, rows[1].Status);
    }

    [Fact]
    public async Task ConcurrentRequests_ForSameSlot_CreateAtMostOneBooking()
    {
        // Arrange — ორი დამოუკიდებელი კონტექსტი = ორი ერთდროული HTTP მოთხოვნა
        await using var db1 = _database.CreateContext();
        await using var db2 = _database.CreateContext();
        var service1 = CreateBookingService(db1);
        var service2 = CreateBookingService(db2);

        // Act
        var results = await Task.WhenAll(
            service1.BookAsync(_studentA, _roomId, Tomorrow, 15),
            service2.BookAsync(_studentB, _roomId, Tomorrow, 15));

        // Assert
        Assert.Equal(1, results.Count(r => r.IsSuccess));
        Assert.Equal(1, results.Count(r => r.Error == ErrorCode.Conflict));
        await using var readDb = _database.CreateContext();
        Assert.Equal(1, await readDb.Bookings.CountAsync(b => b.StartHour == 15));
    }

    [Fact]
    public async Task MyBookings_ReturnsOnlyOwnBookingsWithRoomNames()
    {
        // Arrange
        await using var db = _database.CreateContext();
        var service = CreateBookingService(db);
        await service.BookAsync(_studentA, _roomId, Tomorrow, 9);
        await service.BookAsync(_studentB, _roomId, Tomorrow, 11);

        // Act
        await using var readDb = _database.CreateContext();
        var my = await CreateBookingService(readDb).GetMyBookingsAsync(_studentA);

        // Assert
        var booking = Assert.Single(my.Upcoming);
        Assert.Equal(9, booking.StartHour);
        Assert.Equal("ოთახი 101", booking.Room!.Name);
    }

    [Fact]
    public async Task Statistics_CountsBookingsFromDatabase()
    {
        // Arrange
        await using var db = _database.CreateContext();
        var service = CreateBookingService(db);
        await service.BookAsync(_studentA, _roomId, Tomorrow, 9);
        await service.BookAsync(_studentB, _roomId, Tomorrow, 10);

        // Act
        await using var readDb = _database.CreateContext();
        var stats = new StatisticsService(new RoomRepository(readDb), new BookingRepository(readDb));
        var usage = (await stats.GetRoomUsageAsync(Tomorrow, Tomorrow)).Value!;

        // Assert
        Assert.Equal(2, Assert.Single(usage).BookingCount);
    }
}
