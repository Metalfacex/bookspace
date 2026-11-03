using BookSpace.Core.Abstractions;
using BookSpace.Core.Domain;

namespace BookSpace.UnitTests.Fakes;

public class InMemoryUserRepository : IUserRepository
{
    private int _nextId = 1;
    public List<User> Items { get; } = new();

    public User Seed(string email, string passwordHash = "hashed:Password1", UserRole role = UserRole.Student)
    {
        var user = new User { Id = _nextId++, Email = email, PasswordHash = passwordHash, Role = role };
        Items.Add(user);
        return user;
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(Items.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default) =>
        Task.FromResult(Items.FirstOrDefault(u => u.Email == normalizedEmail));

    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct = default) =>
        Task.FromResult(Items.Any(u => u.Email == normalizedEmail));

    public Task<bool> TryAddAsync(User user, CancellationToken ct = default)
    {
        if (Items.Any(u => u.Email == user.Email))
        {
            return Task.FromResult(false);
        }

        user.Id = _nextId++;
        Items.Add(user);
        return Task.FromResult(true);
    }
}
