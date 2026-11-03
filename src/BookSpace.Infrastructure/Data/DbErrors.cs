using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SqlException = Microsoft.Data.SqlClient.SqlException;

namespace BookSpace.Infrastructure.Data;

internal static class DbErrors
{
    /// <summary>true, თუ შეცდომა unique index-ის დარღვევაა (SQL Server 2601/2627, SQLite 19).</summary>
    public static bool IsUniqueViolation(DbUpdateException ex) => ex.InnerException switch
    {
        SqlException sql => sql.Number is 2601 or 2627,
        SqliteException sqlite => sqlite.SqliteErrorCode == 19,
        _ => false
    };
}
