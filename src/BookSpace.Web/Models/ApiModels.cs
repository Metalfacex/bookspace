using System.ComponentModel.DataAnnotations;
using BookSpace.Core.Domain;
using BookSpace.Core.Services;

namespace BookSpace.Web.Models;

public record RoomDto(int Id, string Name, string Location, int Capacity)
{
    public static RoomDto From(Room r) => new(r.Id, r.Name, r.Location, r.Capacity);
}

public record SlotDto(int StartHour, string Label, string State);

public record AvailabilityDto(int RoomId, DateOnly Date, bool HasFreeSlots, IReadOnlyList<SlotDto> Slots)
{
    public static AvailabilityDto From(DaySchedule s) => new(
        s.RoomId,
        s.Date,
        s.HasFreeSlots,
        s.Slots.Select(x => new SlotDto(x.StartHour, x.Label, x.State.ToString())).ToList());
}

public record BookingDto(int Id, int RoomId, string? RoomName, DateOnly Date, int StartHour, string Status)
{
    public static BookingDto From(Booking b) =>
        new(b.Id, b.RoomId, b.Room?.Name, b.Date, b.StartHour, b.Status.ToString());
}

public class CreateBookingRequest
{
    [Range(1, int.MaxValue)]
    public int RoomId { get; set; }

    [Required]
    public DateOnly? Date { get; set; }

    [Range(0, 23)]
    public int StartHour { get; set; }
}

public record MyBookingsDto(IReadOnlyList<BookingDto> Upcoming, IReadOnlyList<BookingDto> History);

public record ErrorDto(string Error, string Message);
