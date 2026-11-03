using BookSpace.Core.Abstractions;
using BookSpace.Core.Domain;

namespace BookSpace.UnitTests.Fakes;

/// <summary>რეპოზიტორიის ფეიკი, რომელიც ბაზის unique index-ს ასიმულირებს.</summary>
public class InMemoryBookingRepository : IBookingRepository
{
    private int _nextId = 1;
    public List<Booking> Items { get; } = new();
    public int UpdateCalls { get; private set; }

    /// <summary>true-ზე TryAddAsync აბრუნებს false-ს — ასე ვამოწმებთ race condition-ს.</summary>
    public bool SimulateConcurrentInsert { get; set; }

    public Booking Seed(int userId, int roomId, DateOnly date, int startHour,
        BookingStatus status = BookingStatus.Active, User? user = null, Room? room = null)
    {
        var booking = new Booking
        {
            Id = _nextId++,
            UserId = userId,
            RoomId = roomId,
            Date = date,
            StartHour = startHour,
            Status = status,
            User = user,
            Room = room
        };
        Items.Add(booking);
        return booking;
    }

    public Task<Booking?> GetByIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(Items.FirstOrDefault(b => b.Id == id));

    public Task<IReadOnlyList<Booking>> GetActiveForRoomAsync(int roomId, DateOnly date, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Booking>>(Items.Where(b => b.RoomId == roomId && b.Date == date && b.IsActive).ToList());

    public Task<IReadOnlyList<Booking>> GetForUserAsync(int userId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Booking>>(Items.Where(b => b.UserId == userId).ToList());

    public Task<IReadOnlyList<Booking>> GetActiveInRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Booking>>(Items.Where(b => b.Date >= from && b.Date <= to && b.IsActive).ToList());

    public Task<bool> TryAddAsync(Booking booking, CancellationToken ct = default)
    {
        var taken = Items.Any(b => b.IsActive && b.RoomId == booking.RoomId && b.Date == booking.Date &&
                                   b.StartHour == booking.StartHour);
        if (SimulateConcurrentInsert || taken)
        {
            return Task.FromResult(false);
        }

        booking.Id = _nextId++;
        Items.Add(booking);
        return Task.FromResult(true);
    }

    public Task UpdateAsync(Booking booking, CancellationToken ct = default)
    {
        UpdateCalls++;
        return Task.CompletedTask;
    }
}
