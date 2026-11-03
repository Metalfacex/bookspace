namespace BookSpace.ApiTests;

// API-ს JSON პასუხების ფორმა, როგორც კლიენტი ხედავს (ვებ-პროექტის კლასებზე დამოკიდებულების გარეშე).
public record RoomJson(int Id, string Name, string Location, int Capacity);

public record SlotJson(int StartHour, string Label, string State);

public record AvailabilityJson(int RoomId, DateOnly Date, bool HasFreeSlots, List<SlotJson> Slots);

public record BookingJson(int Id, int RoomId, string? RoomName, DateOnly Date, int StartHour, string Status);

public record MyBookingsJson(List<BookingJson> Upcoming, List<BookingJson> History);

public record ErrorJson(string Error, string Message);
