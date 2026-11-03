using Microsoft.AspNetCore.Html;

namespace BookSpace.Web.Ui;

/// <summary>ხაზოვანი SVG აიკონები (inline, გარე ბიბლიოთეკის გარეშე).</summary>
public static class Icons
{
    private static HtmlString Svg(string paths, int size) => new(
        $"<svg width=\"{size}\" height=\"{size}\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" " +
        $"stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\">{paths}</svg>");

    public static HtmlString Book(int size = 20) => Svg(
        "<path d=\"M4 5.5A2.5 2.5 0 0 1 6.5 3H20v15H6.5A2.5 2.5 0 0 0 4 20.5z\"/><path d=\"M4 20.5A2.5 2.5 0 0 0 6.5 23H20v-5\"/><path d=\"M9 7h7\"/>", size);
    public static HtmlString Search(int size = 18) => Svg("<circle cx=\"11\" cy=\"11\" r=\"7\"/><path d=\"m20 20-3.5-3.5\"/>", size);
    public static HtmlString Pin(int size = 16) => Svg("<path d=\"M12 21s-7-6.2-7-11.5a7 7 0 0 1 14 0C19 14.8 12 21 12 21z\"/><circle cx=\"12\" cy=\"9.5\" r=\"2.5\"/>", size);
    public static HtmlString Users(int size = 16) => Svg("<circle cx=\"9\" cy=\"8\" r=\"3.5\"/><path d=\"M2.5 20a6.5 6.5 0 0 1 13 0\"/><path d=\"M16 4.5a3.5 3.5 0 0 1 0 7\"/><path d=\"M18 14.5a6.5 6.5 0 0 1 3.5 5.5\"/>", size);
    public static HtmlString Clock(int size = 16) => Svg("<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 7v5l3 2\"/>", size);
    public static HtmlString Calendar(int size = 16) => Svg("<rect x=\"3.5\" y=\"5\" width=\"17\" height=\"15\" rx=\"2\"/><path d=\"M3.5 10h17M8 3v4M16 3v4\"/>", size);
    public static HtmlString ChevronLeft(int size = 18) => Svg("<path d=\"m15 6-6 6 6 6\"/>", size);
    public static HtmlString ChevronRight(int size = 18) => Svg("<path d=\"m9 6 6 6-6 6\"/>", size);
    public static HtmlString ArrowRight(int size = 16) => Svg("<path d=\"M5 12h14M13 6l6 6-6 6\"/>", size);
    public static HtmlString Check(int size = 16) => Svg("<path d=\"m5 12.5 4.5 4.5L19 7.5\"/>", size);
    public static HtmlString Info(int size = 16) => Svg("<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 11v6M12 7.5v.01\"/>", size);
    public static HtmlString Alert(int size = 16) => Svg("<path d=\"M12 3 2.5 20h19z\"/><path d=\"M12 10v4.5M12 17.5v.01\"/>", size);
    public static HtmlString Bookmark(int size = 16) => Svg("<path d=\"M6 3.5h12V21l-6-4-6 4z\"/>", size);
    public static HtmlString Plus(int size = 16) => Svg("<path d=\"M12 5v14M5 12h14\"/>", size);
    public static HtmlString Edit(int size = 16) => Svg("<path d=\"M4 20h4L19 9l-4-4L4 16z\"/><path d=\"m13.5 6.5 4 4\"/>", size);
    public static HtmlString Chart(int size = 16) => Svg("<path d=\"M4 20V10M10 20V4M16 20v-7M22 20H2\"/>", size);
    public static HtmlString Logout(int size = 16) => Svg("<path d=\"M15 4h4v16h-4\"/><path d=\"M10 8l-4 4 4 4M6 12h10\"/>", size);
    public static HtmlString Menu(int size = 20) => Svg("<path d=\"M4 7h16M4 12h16M4 17h16\"/>", size);
    public static HtmlString Lock(int size = 16) => Svg("<rect x=\"5\" y=\"10.5\" width=\"14\" height=\"10\" rx=\"2\"/><path d=\"M8 10.5V7.5a4 4 0 0 1 8 0v3\"/>", size);
    public static HtmlString Quiet(int size = 16) => Svg("<path d=\"M4 9v6h4l5 4V5L8 9z\"/><path d=\"m17 9 4 6M21 9l-4 6\"/>", size);
}
