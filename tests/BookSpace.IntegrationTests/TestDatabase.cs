using BookSpace.Core.Abstractions;
using BookSpace.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BookSpace.IntegrationTests;

/// <summary>
/// თითოეული ტესტისთვის ცალკე, ცარიელი ბაზა.
/// თუ გარემოს ცვლადი BOOKSPACE_TEST_SQLSERVER მითითებულია (CI-ში — SQL Server კონტეინერი),
/// ტესტები რეალურ MS SQL Server-ზე ეშვება. სხვა შემთხვევაში — დროებით SQLite ფაილზე.
/// </summary>
public sealed class TestDatabase : IAsyncLifetime
{
    public const string SqlServerEnvVar = "BOOKSPACE_TEST_SQLSERVER";

    private readonly string? _sqlServerBase = Environment.GetEnvironmentVariable(SqlServerEnvVar);
    private string _connectionString = string.Empty;
    private string? _sqliteFile;

    public bool IsSqlServer => !string.IsNullOrWhiteSpace(_sqlServerBase);

    public BookSpaceDbContext CreateContext()
    {
        var builder = new DbContextOptionsBuilder<BookSpaceDbContext>();
        if (IsSqlServer)
        {
            builder.UseSqlServer(_connectionString);
        }
        else
        {
            builder.UseSqlite(_connectionString);
        }

        return new BookSpaceDbContext(builder.Options);
    }

    public async Task InitializeAsync()
    {
        if (IsSqlServer)
        {
            var csb = new SqlConnectionStringBuilder(_sqlServerBase)
            {
                InitialCatalog = "BookSpaceTests_" + Guid.NewGuid().ToString("N")
            };
            _connectionString = csb.ConnectionString;
        }
        else
        {
            _sqliteFile = Path.Combine(Path.GetTempPath(), $"bookspace-test-{Guid.NewGuid():N}.db");
            _connectionString = $"Data Source={_sqliteFile};Pooling=False";
        }

        await using var db = CreateContext();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await using (var db = CreateContext())
        {
            await db.Database.EnsureDeletedAsync();
        }

        if (_sqliteFile is not null && File.Exists(_sqliteFile))
        {
            File.Delete(_sqliteFile);
        }
    }
}

public class FixedClock : IClock
{
    public FixedClock(DateTime now)
    {
        Now = now;
    }

    public DateTime Now { get; set; }
}

public class PlainTextHasher : IPasswordHasher
{
    public string Hash(string password) => "plain:" + password;
    public bool Verify(string hash, string password) => hash == "plain:" + password;
}
