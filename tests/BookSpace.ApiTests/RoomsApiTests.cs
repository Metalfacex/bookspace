using System.Net;
using System.Net.Http.Json;

namespace BookSpace.ApiTests;

/// <summary>GET /api/v1/rooms და availability — HTTP დონეზე, რეალური middleware-ით და ბაზით.</summary>
public class RoomsApiTests : IAsyncLifetime
{
    private readonly BookSpaceApiFactory _factory = new();
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _client = ApiClient.CreateClient(_factory);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _factory.DisposeAsync();

    [Fact]
    public async Task GetRooms_Anonymous_Returns200WithSeededRooms()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/rooms");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rooms = await response.Content.ReadFromJsonAsync<List<RoomJson>>();
        Assert.NotNull(rooms);
        Assert.Equal(4, rooms!.Count);
        Assert.All(rooms, r => Assert.True(r.Capacity >= 1));
    }

    [Fact]
    public async Task GetAvailability_Tomorrow_Returns14FreeSlots()
    {
        // Arrange
        var rooms = await _client.GetFromJsonAsync<List<RoomJson>>("/api/v1/rooms");
        var roomId = rooms![0].Id;

        // Act
        var response = await _client.GetAsync($"/api/v1/rooms/{roomId}/availability?date=2026-11-11");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var schedule = await response.Content.ReadFromJsonAsync<AvailabilityJson>();
        Assert.Equal(14, schedule!.Slots.Count);
        Assert.All(schedule.Slots, s => Assert.Equal("Free", s.State));
    }

    [Fact]
    public async Task GetAvailability_UnknownRoom_Returns404WithErrorBody()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/rooms/9999/availability?date=2026-11-11");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorJson>();
        Assert.Equal("NotFound", error!.Error);
    }

    [Fact]
    public async Task OldUnversionedRoute_IsGone()
    {
        // Act
        var response = await _client.GetAsync("/api/rooms");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerDocument_DescribesVersionedEndpoints()
    {
        // Act
        var json = await _client.GetStringAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.Contains("/api/v1/rooms", json);
        Assert.Contains("/api/v1/bookings", json);
        Assert.Contains("BookSpace API", json);
    }
}
