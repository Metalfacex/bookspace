using BookSpace.Core.Domain;

namespace BookSpace.Web.Ui;

/// <summary>ოთახის „ყდის“ ვიზუალური დეტალები: ფერი და ზომის კატეგორია.</summary>
public static class RoomStyle
{
    private const int CoverCount = 6;

    public static string Cover(int roomId) => $"cover-{((roomId % CoverCount) + CoverCount) % CoverCount}";

    public static string SizeKey(int capacity) => capacity switch
    {
        <= 1 => "solo",
        <= 6 => "small",
        _ => "group"
    };

    public static string SizeLabel(int capacity) => SizeKey(capacity) switch
    {
        "solo" => "ინდივიდუალური",
        "small" => "მცირე ჯგუფი",
        _ => "ჯგუფური"
    };

    public static string Seats(int capacity) => capacity == 1 ? "1 ადგილი" : $"{capacity} ადგილი";

    public static bool MatchesSize(Room room, string? size) =>
        string.IsNullOrEmpty(size) || SizeKey(room.Capacity) == size;

    private static readonly string[] MonthsShort =
        { "იან", "თებ", "მარ", "აპრ", "მაი", "ივნ", "ივლ", "აგვ", "სექ", "ოქტ", "ნოე", "დეკ" };

    private static readonly string[] Weekdays =
        { "კვირა", "ორშაბათი", "სამშაბათი", "ოთხშაბათი", "ხუთშაბათი", "პარასკევი", "შაბათი" };

    public static string MonthShort(DateOnly date) => MonthsShort[date.Month - 1];

    public static string Weekday(DateOnly date) => Weekdays[(int)date.DayOfWeek];

    public static string LongDate(DateOnly date) => $"{date.Day} {MonthShort(date)}, {Weekday(date)}";
}
