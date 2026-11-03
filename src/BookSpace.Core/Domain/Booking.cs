namespace BookSpace.Core.Domain;

/// <summary>
/// ერთსაათიანი ჯავშანი: ოთახი + თარიღი + დაწყების საათი (მაგ. 14 = 14:00–15:00).
/// </summary>
public class Booking
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public Room? Room { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateOnly Date { get; set; }
    public int StartHour { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReminderSentAt { get; set; }

    public DateTime StartsAt => Date.ToDateTime(new TimeOnly(StartHour, 0));
    public DateTime EndsAt => StartsAt.AddHours(1);
    public bool IsActive => Status == BookingStatus.Active;
}
