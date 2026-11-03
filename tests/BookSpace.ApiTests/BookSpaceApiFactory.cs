using BookSpace.Core.Abstractions;
using BookSpace.Core.Domain;
using BookSpace.Infrastructure.Data;
using BookSpace.Web.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookSpace.ApiTests;

/// <summary>
/// მთელი აპლიკაცია მეხსიერებაში (TestServer) — HTTP მოთხოვნები გადის რეალურ middleware-ზე,
/// ავტორიზაციაზე, კონტროლერებზე და ბაზაზე. ყოველ factory-ს საკუთარი, ცარიელი ბაზა აქვს:
/// CI-ში MS SQL Server (BOOKSPACE_TEST_SQLSERVER), ლოკალურად SQLite ფაილი.
/// "ახლა" ფიქსირებულია (FixedClock), რომ ტესტები განმეორებადი იყოს.
/// </summary>
public class BookSpaceApiFactory : WebApplicationFactory<Program>
{
    public static readonly DateTime Now = new(2026, 11, 10, 10, 30, 0);
    public static readonly DateOnly Tomorrow = new(2026, 11, 11);

    public const string AdminEmail = "admin@uni.edu.ge";
    public const string AdminPassword = "Admin12345";

    private readonly string _connectionString;
    private readonly bool _sqlServer;
    private readonly string? _sqliteFile;

    public BookSpaceApiFactory()
    {
        var sqlServerBase = Environment.GetEnvironmentVariable("BOOKSPACE_TEST_SQLSERVER");
        _sqlServer = !string.IsNullOrWhiteSpace(sqlServerBase);
        if (_sqlServer)
        {
            _connectionString = new SqlConnectionStringBuilder(sqlServerBase)
            {
                InitialCatalog = "BookSpaceApi_" + Guid.NewGuid().ToString("N")
            }.ConnectionString;
        }
        else
        {
            _sqliteFile = Path.Combine(Path.GetTempPath(), $"bookspace-api-{Guid.NewGuid():N}.db");
            _connectionString = $"Data Source={_sqliteFile};Pooling=False";
        }
    }

    /// <summary>ტესტს შეუძლია rate limit-ის შეცვლა (მაგ. 3 მოთხოვნა წუთში).</summary>
    public int? RateLimitPermit { get; init; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Database:Provider", _sqlServer ? "SqlServer" : "Sqlite");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);
        builder.UseSetting("BookSpace:Reminders:Enabled", "false");

        builder.ConfigureTestServices(services =>
        {
            // ბაზა: ყოველთვის ამ factory-ს ტესტ-ბაზა.
            services.RemoveAll<DbContextOptions<BookSpaceDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.AddDbContext<BookSpaceDbContext>(options =>
            {
                if (_sqlServer)
                {
                    options.UseSqlServer(_connectionString);
                }
                else
                {
                    options.UseSqlite(_connectionString);
                }
            });

            // ფიქსირებული დრო.
            services.RemoveAll<IClock>();
            services.AddSingleton<IClock>(new FixedClock(Now));

            services.Configure<ApiRateLimitOptions>(o => o.PermitLimit = RateLimitPermit ?? 10_000);
        });
    }

    /// <summary>ადმინისტრატორის ანგარიშს პირდაპირ ბაზაში ქმნის.</summary>
    public async Task SeedAdminAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookSpaceDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        if (!await db.Users.AnyAsync(u => u.Email == AdminEmail))
        {
            db.Users.Add(new User
            {
                Email = AdminEmail,
                PasswordHash = hasher.Hash(AdminPassword),
                Role = UserRole.Admin,
                CreatedAt = Now
            });
            await db.SaveChangesAsync();
        }
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            using var scope = Services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<BookSpaceDbContext>().Database.EnsureDeletedAsync();
        }
        catch (InvalidOperationException)
        {
            // სერვერი არ ჩართულა — წასაშლელი არაფერია.
        }

        await base.DisposeAsync();

        if (_sqliteFile is not null && File.Exists(_sqliteFile))
        {
            File.Delete(_sqliteFile);
        }
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTime now)
        {
            Now = now;
        }

        public DateTime Now { get; }
    }
}
