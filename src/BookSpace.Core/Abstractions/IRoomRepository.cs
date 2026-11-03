using BookSpace.Core.Domain;

namespace BookSpace.Core.Abstractions;

public interface IRoomRepository
{
    Task<IReadOnlyList<Room>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<Room?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Room room, CancellationToken ct = default);
    Task UpdateAsync(Room room, CancellationToken ct = default);
}
