using System.Net;

namespace BookSpace.ApiTests;

/// <summary>როლები, CSRF და rate limiting — HTTP დონეზე.</summary>
public class SecurityTests : IAsyncLifetime
{
    private readonly BookSpaceApiFactory _factory = new();

    public Task InitializeAsync() => _factory.SeedAdminAsync();

    public async Task DisposeAsync() => await _factory.DisposeAsync();

    [Fact]
    public async Task Student_OpeningAdminPage_IsSentToAccessDenied()
    {
        // Arrange
        var student = ApiClient.CreateClient(_factory);
        await ApiClient.RegisterAsync(student, "nino@uni.edu.ge");

        // Act
        var response = await student.GetAsync("/Rooms/Create");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task Admin_CanOpenAdminPage()
    {
        // Arrange
        var admin = ApiClient.CreateClient(_factory);
        var login = await ApiClient.LoginAsync(admin, BookSpaceApiFactory.AdminEmail, BookSpaceApiFactory.AdminPassword);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        // Act
        var response = await admin.GetAsync("/Rooms/Create");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_StaysOnPage()
    {
        // Arrange
        var client = ApiClient.CreateClient(_factory);

        // Act
        var response = await ApiClient.LoginAsync(client, BookSpaceApiFactory.AdminEmail, "WrongPass9");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task FormPostWithoutAntiforgeryToken_IsRejected()
    {
        // Arrange
        var client = ApiClient.CreateClient(_factory);
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "x@uni.edu.ge",
            ["Password"] = "Password1",
            ["ConfirmPassword"] = "Password1"
        });

        // Act
        var response = await client.PostAsync("/Account/Register", form);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

/// <summary>Rate limiting ცალკე factory-ით, ლიმიტი = 3 მოთხოვნა ფანჯარაში.</summary>
public class RateLimitTests : IAsyncLifetime
{
    private readonly BookSpaceApiFactory _factory = new() { RateLimitPermit = 3 };

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _factory.DisposeAsync();

    [Fact]
    public async Task FourthRequestInWindow_Returns429()
    {
        // Arrange
        var client = ApiClient.CreateClient(_factory);
        for (var i = 0; i < 3; i++)
        {
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/rooms")).StatusCode);
        }

        // Act
        var response = await client.GetAsync("/api/v1/rooms");

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }
}
