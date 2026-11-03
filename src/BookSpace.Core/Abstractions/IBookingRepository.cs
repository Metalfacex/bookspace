using BookSpace.Core.Domain;

namespace BookSpace.Core.Abstractions;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetActiveForRoomAsync(int roomId, DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetForUserAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetActiveInRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);

    /// <summary>
    /// ინახავს ჯავშანს. აბრუნებს false-ს, თუ იგივე ოთახის იგივე სლოტი უკვე დაკავებულია —
    /// ამას ბაზის unique index ამოწმებს, ასე რომ ერთდროული მოთხოვნებიდან მხოლოდ ერთი გადის.
    /// </summary>
    Task<bool> TryAddAsync(Booking booking, CancellationToken ct = default);

    Task UpdateAsync(Booking booking, CancellationToken ct = default);
}
