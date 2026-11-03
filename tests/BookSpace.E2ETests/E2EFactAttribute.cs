namespace BookSpace.E2ETests;

/// <summary>
/// End-to-End ტესტი გაშვებულ აპლიკაციაზე, რეალურ ბრაუზერში (Playwright, Chromium).
/// ეშვება მხოლოდ მაშინ, როცა BOOKSPACE_E2E_URL მითითებულია (მაგ. http://localhost:5180);
/// სხვა შემთხვევაში გამოტოვებულია, რომ ჩვეულებრივი `dotnet test` არ დაეცეს.
/// </summary>
public sealed class E2EFactAttribute : FactAttribute
{
    public const string UrlVariable = "BOOKSPACE_E2E_URL";

    public E2EFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(UrlVariable)))
        {
            Skip = $"{UrlVariable} is not set — start the app and set it to run E2E tests.";
        }
    }
}
