using System.Net;
using System.Net.Http.Json;

namespace BookSpace.ApiTests;

/// <summary>
/// ჯავშნის სრული სცენარი HTTP-ით: რეგისტრაცია → POST 201 → GET → DELETE 204,
/// ლექციის მაგალითის მსგავსად (POST /api/v1/... → 201, შემდეგ GET).
/// </summary>
public class BookingsApiTests : IAsyncLifetime
{
    private readonly BookSpaceApiFactory _factory = new();
    private HttpClient _student = null!;
    private int _roomId;

    public async Task InitializeAsync()
    {
        _student = ApiClient.CreateClient(_factory);
        var register = await ApiClient.RegisterAsync(_student, "nino@uni.edu.ge");
        Assert.Equal(HttpStatusCode.Redirect, register.StatusCode);

        var rooms = await _student.GetFromJsonAsync<List<RoomJson>>("/api/v1/rooms");
        _roomId = rooms![0].Id;
    }

    public async Task DisposeAsync() => await _factory.DisposeAsync();

    private Task<HttpResponseMessage> BookAsync(HttpClient client, int hour, string date = "2026-11-11") =>
        client.PostAsJsonAsync("/api/v1/bookings", new { roomId = _roomId, date, startHour = hour });

    [Fact]
    public async Task CreateBooking_Returns201WithLocation_ThenGetReturnsIt()
    {
        // Act
        var created = await BookAsync(_student, 10);

        // Assert
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);
        var booking = await created.Content.ReadFromJsonAsync<BookingJson>();
        Assert.Equal(10, booking!.StartHour);
        Assert.Equal("Active", booking.Status);

        var fetched = await _student.GetFromJsonAsync<BookingJson>(created.Headers.Location);
        Assert.Equal(booking.Id, fetched!.Id);
    }

    [Fact]
    public async Task CreateBooking_WithoutLogin_Returns401NotRedirect()
    {
        // Arrange
        var anonymous = ApiClient.CreateClient(_factory);

        // Act
        var response = await BookAsync(anonymous, 10);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(22)]
    public async Task CreateBooking_OutsideOpeningHours_Returns400(int hour)
    {
        // Act
        var response = await BookAsync(_student, hour);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBooking_SlotTakenByAnotherStudent_Returns409()
    {
        // Arrange
        var other = ApiClient.CreateClient(_factory);
        await ApiClient.RegisterAsync(other, "giorgi@uni.edu.ge");
        await BookAsync(other, 14);

        // Act
        var response = await BookAsync(_student, 14);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task FourthBookingSameDay_Returns422()
    {
        // Arrange
        await BookAsync(_student, 9);
        await BookAsync(_student, 11);
        await BookAsync(_student, 13);

        // Act
        var response = await BookAsync(_student, 15);

        // Assert
        Assert.Equal((HttpStatusCode)422, response.StatusCode);
    }

    [Fact]
    public async Task CancelBooking_Returns204_SecondCancelReturns409()
    {
        // Arrange
        var created = await (await BookAsync(_student, 16)).Content.ReadFromJsonAsync<BookingJson>();

        // Act
        var first = await _student.DeleteAsync($"/api/v1/bookings/{created!.Id}");
        var second = await _student.DeleteAsync($"/api/v1/bookings/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task CancelSomeoneElsesBooking_Returns403()
    {
        // Arrange
        var created = await (await BookAsync(_student, 17)).Content.ReadFromJsonAsync<BookingJson>();
        var other = ApiClient.CreateClient(_factory);
        await ApiClient.RegisterAsync(other, "ana@uni.edu.ge");

        // Act
        var response = await other.DeleteAsync($"/api/v1/bookings/{created!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task MyBookings_ReturnsOnlyMine()
    {
        // Arrange
        await BookAsync(_student, 18);
        var other = ApiClient.CreateClient(_factory);
        await ApiClient.RegisterAsync(other, "luka@uni.edu.ge");
        await BookAsync(other, 19);

        // Act
        var mine = await _student.GetFromJsonAsync<MyBookingsJson>("/api/v1/bookings/me");

        // Assert
        var booking = Assert.Single(mine!.Upcoming);
        Assert.Equal(18, booking.StartHour);
    }
}
