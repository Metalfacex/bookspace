using BookSpace.Core.Domain;

namespace BookSpace.Core.Rules;

/// <summary>
/// ამოწმებს სტუდენტის დღიურ ლიმიტს: ერთ დღეში მაქსიმუმ N აქტიური ჯავშანი.
/// TDD — RED: მხოლოდ ჩონჩხი, რომ ტესტები კომპილირდეს და ჩავარდეს.
/// </summary>
public class DailyLimitPolicy
{
    public DailyLimitPolicy(int maxPerDay)
    {
        MaxPerDay = maxPerDay;
    }

    public int MaxPerDay { get; }

    public int RemainingFor(IEnumerable<Booking> userBookings, DateOnly date) => 0;

    public bool CanBook(IEnumerable<Booking> userBookings, DateOnly date) => true;
}
