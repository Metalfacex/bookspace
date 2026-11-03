using BookSpace.Core.Common;
using BookSpace.Core.Domain;
using BookSpace.Core.Services;
using BookSpace.UnitTests.Fakes;

namespace BookSpace.UnitTests.Services;

/// <summary>US-03, US-08, US-09.</summary>
public class RoomServiceTests
{
    private readonly InMemoryRoomRepository _rooms = new();
    private readonly RoomService _sut;

    public RoomServiceTests()
    {
        _sut = new RoomService(_rooms);
    }

    [Fact]
    public async Task Create_AsAdmin_AddsRoomToList()
    {
        // Act
        var result = await _sut.CreateAsync(UserRole.Admin, " ოთახი 305 ", "II კორპუსი", 10);

        // Assert
        Assert.True(result.IsSuccess);
        var list = await _sut.GetActiveRoomsAsync();
        var room = Assert.Single(list);
        Assert.Equal("ოთახი 305", room.Name);
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Manager)]
    public async Task Create_WithoutAdminRole_IsForbidden(UserRole role)
    {
        // Act
        var result = await _sut.CreateAsync(role, "ოთახი 305", "II კორპუსი", 10);

        // Assert
        Assert.Equal(ErrorCode.Forbidden, result.Error);
        Assert.Empty(_rooms.Items);
    }

    [Fact]
    public async Task GetActiveRooms_NoRooms_ReturnsEmptyList()
    {
        // Act
        var list = await _sut.GetActiveRoomsAsync();

        // Assert
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetActiveRooms_HidesInactiveRooms()
    {
        // Arrange
        _rooms.Seed("აქტიური");
        _rooms.Seed("დახურული", isActive: false);

        // Act
        var list = await _sut.GetActiveRoomsAsync();

        // Assert
        Assert.Equal("აქტიური", Assert.Single(list).Name);
    }

    [Fact]
    public async Task Update_ValidData_ChangesRoom()
    {
        // Arrange
        var room = _rooms.Seed(capacity: 6);

        // Act
        var result = await _sut.UpdateAsync(UserRole.Admin, room.Id, "ახალი სახელი", "III კორპუსი", 20, true);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("ახალი სახელი", room.Name);
        Assert.Equal(20, room.Capacity);
    }

    [Fact]
    public async Task Update_ZeroCapacity_IsRejectedAndRoomUnchanged()
    {
        // Arrange
        var room = _rooms.Seed(capacity: 6);

        // Act
        var result = await _sut.UpdateAsync(UserRole.Admin, room.Id, room.Name, room.Location, 0, true);

        // Assert
        Assert.Equal(ErrorCode.Validation, result.Error);
        Assert.Equal(6, room.Capacity);
    }

    [Fact]
    public async Task Update_UnknownRoom_ReturnsNotFound()
    {
        // Act
        var result = await _sut.UpdateAsync(UserRole.Admin, 999, "x", "y", 5, true);

        // Assert
        Assert.Equal(ErrorCode.NotFound, result.Error);
    }
}
