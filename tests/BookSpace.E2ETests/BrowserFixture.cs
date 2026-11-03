using Microsoft.Playwright;

namespace BookSpace.E2ETests;

/// <summary>ერთი ბრაუზერი ყველა ტესტისთვის; ყოველი ტესტი იღებს სუფთა კონტექსტს (cookie-ების გარეშე).</summary>
public sealed class BrowserFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;

    public IBrowser? Browser { get; private set; }

    public string BaseUrl { get; } =
        (Environment.GetEnvironmentVariable(E2EFactAttribute.UrlVariable) ?? "http://localhost:5180").TrimEnd('/');

    public async Task InitializeAsync()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(E2EFactAttribute.UrlVariable)))
        {
            return;
        }

        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    public async Task<IPage> NewPageAsync()
    {
        var context = await Browser!.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
            Locale = "ka-GE",
            ViewportSize = new ViewportSize { Width = 1280, Height = 900 }
        });
        context.SetDefaultTimeout(15_000);
        return await context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.DisposeAsync();
        }

        _playwright?.Dispose();
    }
}

[CollectionDefinition("browser")]
public sealed class BrowserCollection : ICollectionFixture<BrowserFixture>
{
}
