using BookSpace.Core.Abstractions;
using BookSpace.Core.Domain;
using BookSpace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookSpace.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly BookSpaceDbContext _db;

    public RoomRepository(BookSpaceDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Room>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _db.Rooms.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(r => r.IsActive);
        }

        return await query.OrderBy(r => r.Name).ToListAsync(ct);
    }

    public Task<Room?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Rooms.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddAsync(Room room, CancellationToken ct = default)
    {
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Room room, CancellationToken ct = default)
    {
        _db.Rooms.Update(room);
        await _db.SaveChangesAsync(ct);
    }
}
