using BookSpace.Core.Abstractions;
using BookSpace.Core.Domain;
using BookSpace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookSpace.IntegrationTests;

/// <summary>
/// DbInitializer-ის ტესტები — coverage ანგარიშში ეს კლასი 0%-ზე იყო (docs/testing/coverage.md).
/// </summary>
public class DbInitializerTests : IAsyncLifetime
{
    private const string AdminEmailKey = "BookSpace:Seed:AdminEmail";
    private const string AdminPasswordKey = "BookSpace:Seed:AdminPassword";
    private const string ManagerEmailKey = "BookSpace:Seed:ManagerEmail";
    private const string ManagerPasswordKey = "BookSpace:Seed:ManagerPassword";

    private readonly TestDatabase _database = new();

    public Task InitializeAsync() => _database.InitializeAsync();
    public Task DisposeAsync() => _database.DisposeAsync();

    private ServiceProvider BuildServices(Dictionary<string, string?> settings)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(settings).Build());
        services.AddSingleton<IPasswordHasher, PlainTextHasher>();
        services.AddSingleton<IClock>(new FixedClock(new DateTime(2026, 11, 10, 9, 0, 0)));
        services.AddScoped(_ => _database.CreateContext());
        return services.BuildServiceProvider();
    }

    private async Task<int> CountRoomsAsync()
    {
        await using var db = _database.CreateContext();
        return await db.Rooms.CountAsync();
    }

    [Fact]
    public async Task Initialize_RunTwice_DoesNotDuplicateAdminOrRooms()
    {
        // Arrange
        await using var provider = BuildServices(new Dictionary<string, string?>
        {
            [AdminEmailKey] = "admin@uni.edu.ge",
            [AdminPasswordKey] = "Admin12345"
        });

        // Act
        await provider.InitializeBookSpaceDatabaseAsync();
        var roomsAfterFirst = await CountRoomsAsync();
        await provider.InitializeBookSpaceDatabaseAsync();
        var roomsAfterSecond = await CountRoomsAsync();

        // Assert
        await using var db = _database.CreateContext();
        Assert.Equal(1, await db.Users.CountAsync(u => u.Role == UserRole.Admin));
        Assert.True(roomsAfterFirst > 0);
        Assert.Equal(roomsAfterFirst, roomsAfterSecond);
    }

    [Fact]
    public async Task Initialize_AdminAndManager_GetTheirOwnRoles()
    {
        // Arrange
        await using var provider = BuildServices(new Dictionary<string, string?>
        {
            [AdminEmailKey] = "admin@uni.edu.ge",
            [AdminPasswordKey] = "Admin12345",
            [ManagerEmailKey] = "manager@uni.edu.ge",
            [ManagerPasswordKey] = "Manager12345"
        });

        // Act
        await provider.InitializeBookSpaceDatabaseAsync();

        // Assert
        await using var db = _database.CreateContext();
        var users = await db.Users.OrderBy(u => u.Email).ToListAsync();
        Assert.Equal(2, users.Count);
        Assert.Equal(UserRole.Admin, users.Single(u => u.Email == "admin@uni.edu.ge").Role);
        Assert.Equal(UserRole.Manager, users.Single(u => u.Email == "manager@uni.edu.ge").Role);
    }

    [Fact]
    public async Task Initialize_EmptyAdminPassword_DoesNotCreateAdmin()
    {
        // Arrange
        await using var provider = BuildServices(new Dictionary<string, string?>
        {
            [AdminEmailKey] = "admin@uni.edu.ge",
            [AdminPasswordKey] = ""
        });

        // Act
        await provider.InitializeBookSpaceDatabaseAsync();

        // Assert
        await using var db = _database.CreateContext();
        Assert.False(await db.Users.AnyAsync());
    }

    [Fact]
    public async Task Initialize_AdminEmailIsNormalized()
    {
        // Arrange
        await using var provider = BuildServices(new Dictionary<string, string?>
        {
            [AdminEmailKey] = "  Admin@UNI.edu.ge ",
            [AdminPasswordKey] = "Admin12345"
        });

        // Act
        await provider.InitializeBookSpaceDatabaseAsync();

        // Assert
        await using var db = _database.CreateContext();
        var admin = await db.Users.SingleAsync();
        Assert.Equal("admin@uni.edu.ge", admin.Email);
    }

    [Fact]
    public async Task Initialize_ExistingRooms_AreNotOverwritten()
    {
        // Arrange
        await using (var setup = _database.CreateContext())
        {
            setup.Rooms.Add(new Room { Name = "ჩემი ოთახი", Location = "II კორპუსი", Capacity = 5 });
            await setup.SaveChangesAsync();
        }

        await using var provider = BuildServices(new Dictionary<string, string?>());

        // Act
        await provider.InitializeBookSpaceDatabaseAsync();

        // Assert
        await using var db = _database.CreateContext();
        var room = await db.Rooms.SingleAsync();
        Assert.Equal("ჩემი ოთახი", room.Name);
    }
}
