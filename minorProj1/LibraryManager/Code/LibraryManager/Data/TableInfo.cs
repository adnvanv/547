namespace LibraryManager.Data;

/// <summary>Metadata for one user table: its columns, description and current row count.</summary>
public sealed class TableInfo
{
    public required string Schema { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public long RowCount { get; set; }
    public List<ColumnInfo> Columns { get; } = new();

    /// <summary>Name shown to the user; the default dbo schema is omitted.</summary>
    public string DisplayName => ToDisplayName(Schema, Name);

    /// <summary>Bracket-quoted, schema-qualified name for use inside SQL text.</summary>
    public string SqlName => $"{SqlIdentifier.Quote(Schema)}.{SqlIdentifier.Quote(Name)}";

    /// <summary>The single primary key column, or null if the key is missing or composite.</summary>
    public ColumnInfo? PrimaryKey
    {
        get
        {
            var keys = Columns.Where(c => c.IsPrimaryKey).ToList();
            return keys.Count == 1 ? keys[0] : null;
        }
    }

    internal static string ToDisplayName(string schema, string name) =>
        schema.Equals("dbo", StringComparison.OrdinalIgnoreCase) ? name : $"{schema}.{name}";
}
