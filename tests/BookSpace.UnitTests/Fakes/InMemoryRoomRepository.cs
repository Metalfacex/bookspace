using BookSpace.Core.Abstractions;
using BookSpace.Core.Domain;

namespace BookSpace.UnitTests.Fakes;

public class InMemoryRoomRepository : IRoomRepository
{
    private int _nextId = 1;
    public List<Room> Items { get; } = new();

    public Room Seed(string name = "ოთახი 101", string location = "I კორპუსი", int capacity = 6, bool isActive = true)
    {
        var room = new Room { Id = _nextId++, Name = name, Location = location, Capacity = capacity, IsActive = isActive };
        Items.Add(room);
        return room;
    }

    public Task<IReadOnlyList<Room>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Room>>(Items.Where(r => includeInactive || r.IsActive).ToList());

    public Task<Room?> GetByIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(Items.FirstOrDefault(r => r.Id == id));

    public Task AddAsync(Room room, CancellationToken ct = default)
    {
        room.Id = _nextId++;
        Items.Add(room);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Room room, CancellationToken ct = default) => Task.CompletedTask;
}
