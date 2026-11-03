using BookSpace.Core.Abstractions;
using BookSpace.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookSpace.Infrastructure.Data;

public static class DbInitializer
{
    /// <summary>ქმნის სქემას (თუ არ არსებობს) და ამატებს საწყის მონაცემებს.</summary>
    public static async Task InitializeBookSpaceDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookSpaceDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await db.Database.EnsureCreatedAsync();

        await EnsureUserAsync(db, hasher, clock, config["BookSpace:Seed:AdminEmail"],
            config["BookSpace:Seed:AdminPassword"], UserRole.Admin);
        await EnsureUserAsync(db, hasher, clock, config["BookSpace:Seed:ManagerEmail"],
            config["BookSpace:Seed:ManagerPassword"], UserRole.Manager);

        if (!await db.Rooms.AnyAsync())
        {
            db.Rooms.AddRange(
                new Room { Name = "ოთახი 101", Location = "I კორპუსი, 1 სართული", Capacity = 6 },
                new Room { Name = "ოთახი 204", Location = "I კორპუსი, 2 სართული", Capacity = 12 },
                new Room { Name = "ბიბლიოთეკის ჯგუფური ოთახი", Location = "ბიბლიოთეკა, 3 სართული", Capacity = 8 },
                new Room { Name = "ინდივიდუალური კაბინა A", Location = "ბიბლიოთეკა, 1 სართული", Capacity = 1 });
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureUserAsync(BookSpaceDbContext db, IPasswordHasher hasher, IClock clock,
        string? email, string? password, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var normalized = email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == normalized))
        {
            return;
        }

        db.Users.Add(new User
        {
            Email = normalized,
            PasswordHash = hasher.Hash(password),
            Role = role,
            CreatedAt = clock.Now
        });
        await db.SaveChangesAsync();
    }
}
