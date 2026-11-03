using BookSpace.Core.Abstractions;
using BookSpace.Core.Domain;
using BookSpace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookSpace.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly BookSpaceDbContext _db;

    public UserRepository(BookSpaceDbContext db)
    {
        _db = db;
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct = default) =>
        _db.Users.AnyAsync(u => u.Email == normalizedEmail, ct);

    public async Task<bool> TryAddAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Add(user);
        try
        {
            await _db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex) when (DbErrors.IsUniqueViolation(ex))
        {
            _db.Entry(user).State = EntityState.Detached;
            return false;
        }
    }
}
