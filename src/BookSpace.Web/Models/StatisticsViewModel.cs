using BookSpace.Core.Services;

namespace BookSpace.Web.Models;

public class StatisticsViewModel
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public IReadOnlyList<RoomUsage> Usage { get; set; } = Array.Empty<RoomUsage>();
    public string? Error { get; set; }
    public int Total => Usage.Sum(u => u.BookingCount);
}
