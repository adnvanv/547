using System.Data;
using Microsoft.Data.SqlClient;

namespace LibraryManager.Data;

public sealed record ServerInfo(string DatabaseName, string ServerName, string Edition, string Version);

/// <summary>
/// Data layer: every SQL statement the application runs lives here.
/// Table and column names always come from the database's own catalog (never from raw
/// user input) and are bracket-quoted; all values are passed as parameters.
/// </summary>
public sealed class DatabaseRepository
{
    private const string TablesSql = """
        SELECT t.object_id,
               s.name AS SchemaName,
               t.name AS TableName,
               CAST(ep.value AS nvarchar(4000)) AS Description
        FROM sys.tables AS t
        JOIN sys.schemas AS s ON s.schema_id = t.schema_id
        LEFT JOIN sys.extended_properties AS ep
               ON ep.class = 1 AND ep.major_id = t.object_id AND ep.minor_id = 0 AND ep.name = 'MS_Description'
        WHERE t.is_ms_shipped = 0 AND t.name <> 'sysdiagrams';
        """;

    private const string ColumnsSql = """
        SELECT c.object_id,
               c.name AS ColumnName,
               ty.name AS TypeName,
               c.max_length,
               c.precision,
               c.scale,
               c.is_nullable,
               c.is_identity,
               c.is_computed,
               CAST(CASE WHEN c.default_object_id <> 0 THEN 1 ELSE 0 END AS bit) AS HasDefault,
               CAST(CASE WHEN pk.column_id IS NULL THEN 0 ELSE 1 END AS bit) AS IsPrimaryKey,
               rs.name AS RefSchema,
               rt.name AS RefTable,
               rc.name AS RefColumn,
               CAST(ep.value AS nvarchar(4000)) AS Description
        FROM sys.columns AS c
        JOIN sys.tables AS t ON t.object_id = c.object_id
        JOIN sys.types AS ty ON ty.user_type_id = c.user_type_id
        LEFT JOIN (SELECT ic.object_id, ic.column_id
                   FROM sys.indexes AS i
                   JOIN sys.index_columns AS ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                   WHERE i.is_primary_key = 1) AS pk
               ON pk.object_id = c.object_id AND pk.column_id = c.column_id
        LEFT JOIN sys.foreign_key_columns AS fkc
               ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
        LEFT JOIN sys.tables AS rt ON rt.object_id = fkc.referenced_object_id
        LEFT JOIN sys.schemas AS rs ON rs.schema_id = rt.schema_id
        LEFT JOIN sys.columns AS rc
               ON rc.object_id = fkc.referenced_object_id AND rc.column_id = fkc.referenced_column_id
        LEFT JOIN sys.extended_properties AS ep
               ON ep.class = 1 AND ep.major_id = c.object_id AND ep.minor_id = c.column_id AND ep.name = 'MS_Description'
        WHERE t.is_ms_shipped = 0
        ORDER BY c.object_id, c.column_id;
        """;

    private readonly string _connectionString;

    public DatabaseRepository(string connectionString) => _connectionString = connectionString;

    public ServerInfo GetServerInfo()
    {
        const string sql = """
            SELECT DB_NAME(),
                   CAST(@@SERVERNAME AS nvarchar(128)),
                   CAST(SERVERPROPERTY('Edition') AS nvarchar(128)),
                   CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(128));
            """;
        using var connection = OpenConnection();
        using var command = CreateCommand(connection, sql);
        using var reader = command.ExecuteReader();
        reader.Read();
        return new ServerInfo(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
    }

    /// <summary>Reads every user table with its columns, keys, descriptions and row count.</summary>
    public List<TableInfo> GetTables()
    {
        using var connection = OpenConnection();
        var tables = new Dictionary<int, TableInfo>();

        using (var command = CreateCommand(connection, TablesSql))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                tables[reader.GetInt32(0)] = new TableInfo
                {
                    Schema = reader.GetString(1),
                    Name = reader.GetString(2),
                    Description = GetNullableString(reader, 3)
                };
            }
        }

        using (var command = CreateCommand(connection, ColumnsSql))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                if (!tables.TryGetValue(reader.GetInt32(0), out var table))
                    continue;

                var name = reader.GetString(1);
                if (table.Columns.Any(c => c.Name == name))
                    continue; // same column listed again because it is part of more than one foreign key

                table.Columns.Add(new ColumnInfo
                {
                    Name = name,
                    SqlType = reader.GetString(2),
                    MaxLength = reader.GetInt16(3),
                    Precision = reader.GetByte(4),
                    Scale = reader.GetByte(5),
                    IsNullable = reader.GetBoolean(6),
                    IsIdentity = reader.GetBoolean(7),
                    IsComputed = reader.GetBoolean(8),
                    HasDefault = reader.GetBoolean(9),
                    IsPrimaryKey = reader.GetBoolean(10),
                    ReferencedSchema = GetNullableString(reader, 11),
                    ReferencedTable = GetNullableString(reader, 12),
                    ReferencedColumn = GetNullableString(reader, 13),
                    Description = GetNullableString(reader, 14)
                });
            }
        }

        foreach (var table in tables.Values)
        {
            using var command = CreateCommand(connection, $"SELECT COUNT_BIG(*) FROM {table.SqlName};");
            table.RowCount = (long)command.ExecuteScalar()!;
        }

        return tables.Values.OrderBy(t => t.Schema).ThenBy(t => t.Name).ToList();
    }

    public long CountRows(TableInfo table)
    {
        using var connection = OpenConnection();
        using var command = CreateCommand(connection, $"SELECT COUNT_BIG(*) FROM {table.SqlName};");
        return (long)command.ExecuteScalar()!;
    }

    public DataTable GetAllRows(TableInfo table)
    {
        var orderBy = table.PrimaryKey is { } pk ? $" ORDER BY {SqlIdentifier.Quote(pk.Name)}" : "";
        return Query($"SELECT * FROM {table.SqlName}{orderBy};");
    }

    public DataTable GetRowById(TableInfo table, object id)
    {
        var pk = RequirePrimaryKey(table);
        return Query($"SELECT * FROM {table.SqlName} WHERE {SqlIdentifier.Quote(pk.Name)} = @id;", ("@id", id));
    }

    /// <summary>Returns rows from the table a foreign key column points to, so the user can see valid values.</summary>
    public DataTable GetReferencedRows(ColumnInfo foreignKey, int maxRows)
    {
        if (!foreignKey.IsForeignKey)
            throw new ArgumentException($"{foreignKey.Name} is not a foreign key column.", nameof(foreignKey));

        var sql = $"SELECT TOP (@maxRows) * FROM {SqlIdentifier.Quote(foreignKey.ReferencedSchema!)}.{SqlIdentifier.Quote(foreignKey.ReferencedTable!)} " +
                  $"ORDER BY {SqlIdentifier.Quote(foreignKey.ReferencedColumn!)};";
        return Query(sql, ("@maxRows", maxRows));
    }

    /// <summary>Inserts a row and returns the new record's primary key value.</summary>
    /// <param name="values">Columns to set. Columns not listed get their default (or NULL).</param>
    public object InsertRow(TableInfo table, IReadOnlyList<(ColumnInfo Column, object? Value)> values)
    {
        var pk = RequirePrimaryKey(table);
        var output = $"OUTPUT INSERTED.{SqlIdentifier.Quote(pk.Name)}";
        var parameters = values.Select((v, i) => ($"@p{i}", v.Value)).ToArray();

        var sql = values.Count == 0
            ? $"INSERT INTO {table.SqlName} {output} DEFAULT VALUES;"
            : $"INSERT INTO {table.SqlName} ({string.Join(", ", values.Select(v => SqlIdentifier.Quote(v.Column.Name)))}) " +
              $"{output} VALUES ({string.Join(", ", parameters.Select(p => p.Item1))});";

        using var connection = OpenConnection();
        using var command = CreateCommand(connection, sql, parameters);
        return command.ExecuteScalar()!;
    }

    /// <summary>Sets one column of the record with the given primary key. Returns rows affected.</summary>
    public int UpdateColumn(TableInfo table, object id, ColumnInfo column, object? value)
    {
        var pk = RequirePrimaryKey(table);
        var sql = $"UPDATE {table.SqlName} SET {SqlIdentifier.Quote(column.Name)} = @value " +
                  $"WHERE {SqlIdentifier.Quote(pk.Name)} = @id;";
        return Execute(sql, ("@value", value), ("@id", id));
    }

    /// <summary>Deletes the record with the given primary key. Returns rows affected.</summary>
    public int DeleteRow(TableInfo table, object id)
    {
        var pk = RequirePrimaryKey(table);
        return Execute($"DELETE FROM {table.SqlName} WHERE {SqlIdentifier.Quote(pk.Name)} = @id;", ("@id", id));
    }

    private SqlConnection OpenConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static SqlCommand CreateCommand(SqlConnection connection, string sql, params (string Name, object? Value)[] parameters)
    {
        var command = new SqlCommand(sql, connection);
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        return command;
    }

    private DataTable Query(string sql, params (string Name, object? Value)[] parameters)
    {
        using var connection = OpenConnection();
        using var command = CreateCommand(connection, sql, parameters);
        using var reader = command.ExecuteReader();
        var result = new DataTable();
        result.Load(reader);
        return result;
    }

    private int Execute(string sql, params (string Name, object? Value)[] parameters)
    {
        using var connection = OpenConnection();
        using var command = CreateCommand(connection, sql, parameters);
        return command.ExecuteNonQuery();
    }

    private static ColumnInfo RequirePrimaryKey(TableInfo table) =>
        table.PrimaryKey ?? throw new InvalidOperationException($"{table.DisplayName} has no single-column primary key.");

    private static string? GetNullableString(SqlDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
}

internal static class SqlIdentifier
{
    /// <summary>Quotes a SQL Server identifier, e.g. Order Details -> [Order Details].</summary>
    public static string Quote(string name) => $"[{name.Replace("]", "]]")}]";
}
