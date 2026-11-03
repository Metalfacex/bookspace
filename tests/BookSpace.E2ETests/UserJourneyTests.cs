using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace BookSpace.E2ETests;

/// <summary>
/// მომხმარებლის სრული სცენარები ბრაუზერში — ტესტების პირამიდის მწვერვალი (ცოტა, ნელი, მთელი სისტემა).
/// სელექტორები data-testid-ით, ლოდინი ელემენტზე (sleep-ის გარეშე) — flaky ტესტების თავიდან ასაცილებლად.
/// </summary>
[Collection("browser")]
public class UserJourneyTests
{
    private readonly BrowserFixture _browser;

    public UserJourneyTests(BrowserFixture browser)
    {
        _browser = browser;
    }

    [E2EFact]
    public async Task Visitor_SearchesFromHomePage_SeesOnlyMatchingRooms()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        await page.GotoAsync("/");

        // Act
        await page.FillAsync("#hero-q", "ბიბლიოთეკა");
        await page.ClickAsync(".hero-search button[type=submit]");
        await page.WaitForURLAsync(new Regex("/Rooms"));

        // Assert
        var cards = page.GetByTestId("room-card");
        await Assertions.Expect(cards.First).ToBeVisibleAsync();
        var count = await cards.CountAsync();
        Assert.True(count >= 1);
        for (var i = 0; i < count; i++)
        {
            Assert.Contains("ბიბლიოთეკ", await cards.Nth(i).InnerTextAsync());
        }
    }

    [E2EFact]
    public async Task Visitor_OpensSchedule_IsAskedToLogInBeforeBooking()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        await page.GotoAsync("/Rooms");

        // Act
        await page.GetByTestId("room-card").First.GetByRole(AriaRole.Link, new() { Name = "განრიგი", Exact = true }).ClickAsync();

        // Assert
        await Assertions.Expect(page.GetByTestId("login-to-book")).ToBeVisibleAsync();
        Assert.Equal(0, await page.GetByTestId("book-slot").CountAsync());
    }

    [E2EFact]
    public async Task Student_RegistersBooksAndCancels()
    {
        // Arrange — ახალი სტუდენტი
        var page = await _browser.NewPageAsync();
        var email = $"e2e-{Guid.NewGuid():N}@uni.edu.ge";
        await page.GotoAsync("/Account/Register");
        await page.FillAsync("#Email", email);
        await page.FillAsync("#Password", "Password1");
        await page.FillAsync("#ConfirmPassword", "Password1");
        await page.ClickAsync("form button[type=submit]");
        await page.WaitForURLAsync(new Regex("/Rooms"));

        // Act 1 — ხვალინდელი თავისუფალი სლოტის დაჯავშნა
        await page.GetByTestId("room-card").First.GetByRole(AriaRole.Link, new() { Name = "განრიგი", Exact = true }).ClickAsync();
        await page.GetByLabel("შემდეგი დღე").ClickAsync();
        await page.GetByTestId("book-slot").First.ClickAsync();

        // Assert 1
        await Assertions.Expect(page.Locator(".alert-success")).ToContainTextAsync("დაჯავშნილია");

        // Act 2 — ჩემს ჯავშნებში ნახვა და გაუქმება
        await page.GotoAsync("/Bookings/My");
        await Assertions.Expect(page.GetByTestId("upcoming-booking")).ToHaveCountAsync(1);
        page.Dialog += (_, dialog) => dialog.AcceptAsync();
        await page.GetByTestId("cancel-booking").ClickAsync();

        // Assert 2
        await Assertions.Expect(page.Locator(".alert-success")).ToContainTextAsync("გაუქმდა");
        await Assertions.Expect(page.GetByTestId("upcoming-booking")).ToHaveCountAsync(0);
    }
}
