using BookSpace.Core.Domain;

namespace BookSpace.Core.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct = default);

    /// <summary>ამატებს მომხმარებელს. აბრუნებს false-ს, თუ ელფოსტა უკვე დაკავებულია (unique index).</summary>
    Task<bool> TryAddAsync(User user, CancellationToken ct = default);
}
