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

    [Fact]
    public async Task Initialize_RunTwice_DoesNotDuplicateAdminOrRooms()
    {
        // Arrange
        await using var provider = BuildServices(new Dictionary<string, string?>
        {
            ["BookSpace:Seed:AdminEmail"] = "admin@uni.edu.ge",
            ["BookSpace:Seed:AdminPassword"] = "Admin12345"
        });

        // Act
        await provider.InitializeBookSpaceDatabaseAsync();
        await provider.InitializeBookSpaceDatabaseAsync();

        // Assert
        await using var db = _database.CreateContext();
        Assert.Equal(1, await db.Users.CountAsync(u => u.Role == UserRole.Admin));
        Assert.Equal(4, await db.Rooms.CountAsync());
    }

    [Fact]
    public async Task Initialize_EmptyAdminPassword_DoesNotCreateAdmin()
    {
        // Arrange
        await using var provider = BuildServices(new Dictionary<string, string?>
        {
            ["BookSpace:Seed:AdminEmail"] = "admin@uni.edu.ge",
            ["BookSpace:Seed:AdminPassword"] = ""
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
            ["BookSpace:Seed:AdminEmail"] = "  Admin@UNI.edu.ge ",
            ["BookSpace:Seed:AdminPassword"] = "Admin12345"
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
