using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelReservations.Data;

/// <summary>Database errors worded for the user (e.g. a delete blocked by a foreign key).</summary>
public sealed class DataLayerException(string message, Exception? inner = null) : Exception(message, inner);

/// <summary>
/// Thin ADO.NET helper: opens a connection per call, runs parameterized SQL and maps rows.
/// Every repository goes through here so SQL Server errors are translated in one place.
/// </summary>
public class SqlDatabase(string connectionString)
{
    public const string DefaultConnectionString =
        @"Server=.\SQLEXPRESS;Database=HotelReservationsDB;Integrated Security=True;TrustServerCertificate=True;";

    public async Task<List<T>> QueryAsync<T>(string sql, Func<SqlDataReader, T> map,
                                             params SqlParameter[] parameters)
    {
        await using var connection = new SqlConnection(connectionString);
        await using var command = CreateCommand(connection, sql, parameters);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        var results = new List<T>();
        while (await reader.ReadAsync())
            results.Add(map(reader));
        return results;
    }

    public async Task<T?> QuerySingleAsync<T>(string sql, Func<SqlDataReader, T> map,
                                              params SqlParameter[] parameters) where T : class =>
        (await QueryAsync(sql, map, parameters)).FirstOrDefault();

    public async Task<object?> ScalarAsync(string sql, params SqlParameter[] parameters)
    {
        await using var connection = new SqlConnection(connectionString);
        await using var command = CreateCommand(connection, sql, parameters);
        await connection.OpenAsync();
        var value = await command.ExecuteScalarAsync();
        return value is DBNull ? null : value;
    }

    /// <summary>Runs an INSERT/UPDATE/DELETE and returns the number of rows affected.</summary>
    public async Task<int> ExecuteAsync(string sql, string inUseMessage, params SqlParameter[] parameters)
    {
        try
        {
            await using var connection = new SqlConnection(connectionString);
            await using var command = CreateCommand(connection, sql, parameters);
            await connection.OpenAsync();
            return await command.ExecuteNonQueryAsync();
        }
        catch (SqlException ex)
        {
            throw Translate(ex, inUseMessage);
        }
    }

    /// <summary>Runs an INSERT ... OUTPUT INSERTED.Id statement and returns the new id.</summary>
    public async Task<int> InsertAsync(string sql, string inUseMessage, params SqlParameter[] parameters)
    {
        try
        {
            return Convert.ToInt32(await ScalarAsync(sql, parameters));
        }
        catch (SqlException ex)
        {
            throw Translate(ex, inUseMessage);
        }
    }

    private static DataLayerException Translate(SqlException ex, string inUseMessage) => ex.Number switch
    {
        547 => new DataLayerException(inUseMessage, ex),                        // FK / CHECK constraint
        2627 or 2601 => new DataLayerException(                                 // UNIQUE constraint
            "Another record already uses that value (emails, room numbers and room type names must be unique).", ex),
        _ => new DataLayerException("The database rejected the change: " + ex.Message, ex),
    };

    public static void EnsureOneRow(int rows)
    {
        if (rows == 0)
            throw new DataLayerException("That record no longer exists. It may have been deleted already.");
    }

    private static SqlCommand CreateCommand(SqlConnection connection, string sql, SqlParameter[] parameters)
    {
        var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };
        command.Parameters.AddRange(parameters);
        return command;
    }

    // Parameter helpers so nullable values become DBNull and types are explicit.
    public static SqlParameter P(string name, string? value, int size) =>
        new(name, SqlDbType.NVarChar, size) { Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim() };

    public static SqlParameter P(string name, int value) => new(name, SqlDbType.Int) { Value = value };

    public static SqlParameter P(string name, bool value) => new(name, SqlDbType.Bit) { Value = value };

    public static SqlParameter P(string name, decimal value) =>
        new(name, SqlDbType.Decimal) { Precision = 8, Scale = 2, Value = value };

    public static SqlParameter P(string name, DateOnly value) => new(name, SqlDbType.Date) { Value = value };
}

/// <summary>Null-safe column readers.</summary>
internal static class ReaderExtensions
{
    public static string? GetNullableString(this SqlDataReader r, string column)
    {
        var i = r.GetOrdinal(column);
        return r.IsDBNull(i) ? null : r.GetString(i);
    }

    public static string Str(this SqlDataReader r, string column) => r.GetString(r.GetOrdinal(column));
    public static int Int(this SqlDataReader r, string column) => r.GetInt32(r.GetOrdinal(column));
    public static bool Bool(this SqlDataReader r, string column) => r.GetBoolean(r.GetOrdinal(column));
    public static decimal Dec(this SqlDataReader r, string column) => r.GetDecimal(r.GetOrdinal(column));
    public static DateOnly Date(this SqlDataReader r, string column) =>
        r.GetFieldValue<DateOnly>(r.GetOrdinal(column));
}
