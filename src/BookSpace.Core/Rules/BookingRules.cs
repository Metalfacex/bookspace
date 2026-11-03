namespace BookSpace.Core.Rules;

/// <summary>დაჯავშნის წესები. მნიშვნელობები appsettings.json-ის "BookSpace:Booking" სექციიდან იკითხება.</summary>
public class BookingRules
{
    /// <summary>პირველი სლოტის დაწყების საათი (ჩათვლით).</summary>
    public int OpeningHour { get; set; } = 8;

    /// <summary>დახურვის საათი: ბოლო სლოტი იწყება ClosingHour - 1-ზე.</summary>
    public int ClosingHour { get; set; } = 22;

    /// <summary>ერთ სტუდენტს ერთ დღეში მაქსიმუმ ამდენი აქტიური ჯავშანი (საათი) შეუძლია.</summary>
    public int MaxBookingsPerDay { get; set; } = 3;

    /// <summary>რამდენი დღით წინ შეიძლება დაჯავშნა (დღეს = 0).</summary>
    public int MaxDaysAhead { get; set; } = 14;

    /// <summary>შეხსენება იგზავნება დაწყებამდე ამდენი წუთით ადრე.</summary>
    public int ReminderMinutesBefore { get; set; } = 60;

    public bool IsWithinOpeningHours(int startHour) => startHour >= OpeningHour && startHour < ClosingHour;
}
