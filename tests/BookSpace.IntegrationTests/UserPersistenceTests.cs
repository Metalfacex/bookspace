using BookSpace.Core.Common;
using BookSpace.Core.Rules;
using BookSpace.Core.Services;
using BookSpace.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BookSpace.IntegrationTests;

public class UserPersistenceTests : IAsyncLifetime
{
    private readonly TestDatabase _database = new();

    public Task InitializeAsync() => _database.InitializeAsync();
    public Task DisposeAsync() => _database.DisposeAsync();

    [Fact]
    public async Task Register_ThenLogin_WorksAgainstDatabase()
    {
        // Arrange
        await using var db = _database.CreateContext();
        var auth = new AuthService(new UserRepository(db), new PlainTextHasher(),
            new FixedClock(new DateTime(2026, 11, 10, 10, 0, 0)), new AccountRules());

        // Act
        var registered = await auth.RegisterAsync("Ana@Uni.Edu.Ge", "Password1");
        var loggedIn = await auth.LoginAsync("ana@uni.edu.ge", "Password1");

        // Assert
        Assert.True(registered.IsSuccess);
        Assert.True(loggedIn.IsSuccess);
        Assert.Equal(registered.Value!.Id, loggedIn.Value!.Id);
    }

    [Fact]
    public async Task UniqueEmailIndex_BlocksDuplicateEvenWithoutServiceCheck()
    {
        // Arrange
        await using var db = _database.CreateContext();
        var repo = new UserRepository(db);
        await repo.TryAddAsync(new BookSpace.Core.Domain.User { Email = "ana@uni.edu.ge", PasswordHash = "x" });

        // Act
        var duplicate = await repo.TryAddAsync(new BookSpace.Core.Domain.User { Email = "ana@uni.edu.ge", PasswordHash = "y" });

        // Assert
        Assert.False(duplicate);
        await using var readDb = _database.CreateContext();
        Assert.Equal(1, await readDb.Users.CountAsync());
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        await using var db = _database.CreateContext();
        var auth = new AuthService(new UserRepository(db), new PlainTextHasher(),
            new FixedClock(new DateTime(2026, 11, 10, 10, 0, 0)), new AccountRules());
        await auth.RegisterAsync("ana@uni.edu.ge", "Password1");

        // Act
        var second = await auth.RegisterAsync("ana@uni.edu.ge", "Password2");

        // Assert
        Assert.Equal(ErrorCode.Conflict, second.Error);
    }
}
