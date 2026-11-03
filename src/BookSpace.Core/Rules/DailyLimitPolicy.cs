using BookSpace.Core.Domain;

namespace BookSpace.Core.Rules;

/// <summary>
/// ამოწმებს სტუდენტის დღიურ ლიმიტს: ერთ დღეში მაქსიმუმ N აქტიური ჯავშანი.
/// </summary>
public class DailyLimitPolicy
{
    public DailyLimitPolicy(int maxPerDay)
    {
        if (maxPerDay < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPerDay), "ლიმიტი მინიმუმ 1 უნდა იყოს.");
        }

        MaxPerDay = maxPerDay;
    }

    public int MaxPerDay { get; }

    public int RemainingFor(IEnumerable<Booking> userBookings, DateOnly date)
    {
        var count = 0;
        foreach (var b in userBookings)
        {
            if (b.Status == BookingStatus.Active && b.Date == date) count++;
        }

        return MaxPerDay - count < 0 ? 0 : MaxPerDay - count;
    }

    public bool CanBook(IEnumerable<Booking> userBookings, DateOnly date)
    {
        var count = 0;
        foreach (var b in userBookings)
        {
            if (b.Status == BookingStatus.Active && b.Date == date) count++;
        }

        return count < MaxPerDay;
    }
}
