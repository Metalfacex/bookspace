using BookSpace.Core.Domain;

namespace BookSpace.Core.Rules;

/// <summary>
/// ამოწმებს სტუდენტის დღიურ ლიმიტს: ერთ დღეში მაქსიმუმ N აქტიური ჯავშანი.
/// დაიწერა TDD ციკლით (იხ. docs/testing/tdd-cycle.md).
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
        var used = userBookings.Count(b => b.IsActive && b.Date == date);
        return Math.Max(0, MaxPerDay - used);
    }

    public bool CanBook(IEnumerable<Booking> userBookings, DateOnly date) => RemainingFor(userBookings, date) > 0;
}
