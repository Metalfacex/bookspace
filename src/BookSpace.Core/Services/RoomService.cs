using BookSpace.Core.Abstractions;
using BookSpace.Core.Common;
using BookSpace.Core.Domain;
using BookSpace.Core.Validation;

namespace BookSpace.Core.Services;

/// <summary>US-03 ოთახების სია, US-08 დამატება, US-09 რედაქტირება.</summary>
public class RoomService
{
    private readonly IRoomRepository _rooms;

    public RoomService(IRoomRepository rooms)
    {
        _rooms = rooms;
    }

    public Task<IReadOnlyList<Room>> GetActiveRoomsAsync(CancellationToken ct = default) =>
        _rooms.GetAllAsync(includeInactive: false, ct);

    public Task<IReadOnlyList<Room>> GetAllRoomsAsync(CancellationToken ct = default) =>
        _rooms.GetAllAsync(includeInactive: true, ct);

    public Task<Room?> GetAsync(int id, CancellationToken ct = default) => _rooms.GetByIdAsync(id, ct);

    public async Task<Result<Room>> CreateAsync(UserRole actorRole, string? name, string? location, int capacity,
        CancellationToken ct = default)
    {
        if (actorRole != UserRole.Admin)
        {
            return Result<Room>.Fail(ErrorCode.Forbidden, "ოთახის დამატება მხოლოდ ადმინისტრატორს შეუძლია.");
        }

        var errors = RoomValidator.Validate(name, location, capacity);
        if (errors.Count > 0)
        {
            return Result<Room>.Fail(ErrorCode.Validation, string.Join(" ", errors));
        }

        var room = new Room
        {
            Name = name!.Trim(),
            Location = location!.Trim(),
            Capacity = capacity,
            IsActive = true
        };

        await _rooms.AddAsync(room, ct);
        return Result<Room>.Ok(room);
    }

    public async Task<Result<Room>> UpdateAsync(UserRole actorRole, int roomId, string? name, string? location,
        int capacity, bool isActive, CancellationToken ct = default)
    {
        if (actorRole != UserRole.Admin)
        {
            return Result<Room>.Fail(ErrorCode.Forbidden, "ოთახის რედაქტირება მხოლოდ ადმინისტრატორს შეუძლია.");
        }

        var room = await _rooms.GetByIdAsync(roomId, ct);
        if (room is null)
        {
            return Result<Room>.Fail(ErrorCode.NotFound, "ოთახი ვერ მოიძებნა.");
        }

        var errors = RoomValidator.Validate(name, location, capacity);
        if (errors.Count > 0)
        {
            return Result<Room>.Fail(ErrorCode.Validation, string.Join(" ", errors));
        }

        room.Name = name!.Trim();
        room.Location = location!.Trim();
        room.Capacity = capacity;
        room.IsActive = isActive;

        await _rooms.UpdateAsync(room, ct);
        return Result<Room>.Ok(room);
    }
}
