namespace BookSpace.Core.Services;

public enum SlotState
{
    Free,
    Booked,
    Past
}

public record TimeSlot(int StartHour, SlotState State)
{
    public bool IsFree => State == SlotState.Free;
    public string Label => $"{StartHour:00}:00–{StartHour + 1:00}:00";
}

public record DaySchedule(int RoomId, DateOnly Date, IReadOnlyList<TimeSlot> Slots)
{
    public bool HasFreeSlots => Slots.Any(s => s.IsFree);
}
