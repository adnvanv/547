using System.Data;
using Microsoft.Data.SqlClient;

namespace VetClinic.Data;

/// <summary>Thrown for database errors that should be shown to the user as-is.</summary>
public sealed class DataLayerException(string message, Exception? inner = null) : Exception(message, inner);

/// <summary>
/// Thin ADO.NET helper shared by the repositories. Every value goes through
/// <see cref="SqlParameter"/>s; SQL text is never built from user input.
/// </summary>
public sealed class SqlDatabase(string connectionString)
{
    public const string DefaultConnectionString =
        @"Server=.\SQLEXPRESS;Database=VetClinicDB;Integrated Security=True;TrustServerCertificate=True;";

    // SQL Server error numbers we translate into friendlier messages.
    private const int ConstraintViolation = 547; // foreign key or CHECK constraint
    private const int UniqueViolation = 2627;

    public string ConnectionString { get; } = connectionString;

    public void TestConnection()
    {
        Run(() =>
        {
            using var connection = Open();
            return 0;
        });
    }

    public List<T> Query<T>(string sql, Func<SqlDataReader, T> map, params SqlParameter[] parameters) =>
        Run(() =>
        {
            using var connection = Open();
            using var command = CreateCommand(connection, sql, parameters);
            using var reader = command.ExecuteReader();
            var rows = new List<T>();
            while (reader.Read())
                rows.Add(map(reader));
            return rows;
        });

    public int Execute(string sql, params SqlParameter[] parameters) =>
        Run(() =>
        {
            using var connection = Open();
            using var command = CreateCommand(connection, sql, parameters);
            return command.ExecuteNonQuery();
        });

    public T Scalar<T>(string sql, params SqlParameter[] parameters) =>
        Run(() =>
        {
            using var connection = Open();
            using var command = CreateCommand(connection, sql, parameters);
            return (T)Convert.ChangeType(command.ExecuteScalar()!, typeof(T));
        });

    public static SqlParameter Param(string name, SqlDbType type, object? value, int size = 0)
    {
        var parameter = new SqlParameter(name, type) { Value = value ?? DBNull.Value };
        if (size > 0)
            parameter.Size = size;
        return parameter;
    }

    /// <summary>Trims text and turns blank strings into NULL.</summary>
    public static SqlParameter Text(string name, string? value, int size) =>
        Param(name, SqlDbType.NVarChar, string.IsNullOrWhiteSpace(value) ? null : value.Trim(), size);

    public static string? NullableString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    public static decimal? NullableDecimal(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
    }

    public static DateTime? NullableDate(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }

    private SqlConnection Open()
    {
        var connection = new SqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    private static SqlCommand CreateCommand(SqlConnection connection, string sql, SqlParameter[] parameters)
    {
        var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };
        command.Parameters.AddRange(parameters);
        return command;
    }

    private static T Run<T>(Func<T> action)
    {
        try
        {
            return action();
        }
        catch (SqlException ex) when (ex.Number == ConstraintViolation && ex.Message.Contains("REFERENCE"))
        {
            throw new DataLayerException(
                "This record is referenced by other records and cannot be changed or deleted.", ex);
        }
        catch (SqlException ex) when (ex.Number == ConstraintViolation && ex.Message.Contains("CHECK"))
        {
            throw new DataLayerException("One of the values is outside the allowed range.", ex);
        }
        catch (SqlException ex) when (ex.Number == UniqueViolation)
        {
            throw new DataLayerException("A record with the same key already exists.", ex);
        }
        catch (SqlException ex)
        {
            throw new DataLayerException($"Database error: {ex.Message}", ex);
        }
    }
}
