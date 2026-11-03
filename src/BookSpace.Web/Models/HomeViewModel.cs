using BookSpace.Core.Domain;

namespace BookSpace.Web.Models;

public class HomeViewModel
{
    public IReadOnlyList<Room> Rooms { get; init; } = Array.Empty<Room>();
    public int RoomCount => Rooms.Count;
    public int TotalSeats => Rooms.Sum(r => r.Capacity);
}

public class RoomListViewModel
{
    public IReadOnlyList<Room> Rooms { get; init; } = Array.Empty<Room>();
    public int TotalCount { get; init; }
    public string? Query { get; init; }
    public string? Size { get; init; }
    public bool IsFiltered => !string.IsNullOrWhiteSpace(Query) || !string.IsNullOrEmpty(Size);
}
