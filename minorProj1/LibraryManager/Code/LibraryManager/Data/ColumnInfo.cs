using System.Globalization;

namespace LibraryManager.Data;

/// <summary>Metadata for one table column, read from the SQL Server catalog views.</summary>
public sealed class ColumnInfo
{
    public required string Name { get; init; }
    public required string SqlType { get; init; }

    /// <summary>Storage size in bytes (nvarchar uses 2 bytes per character); -1 means MAX.</summary>
    public short MaxLength { get; init; }
    public byte Precision { get; init; }
    public byte Scale { get; init; }
    public bool IsNullable { get; init; }
    public bool IsIdentity { get; init; }
    public bool IsComputed { get; init; }
    public bool IsPrimaryKey { get; init; }
    public bool HasDefault { get; init; }
    public string? Description { get; init; }

    public string? ReferencedSchema { get; init; }
    public string? ReferencedTable { get; init; }
    public string? ReferencedColumn { get; init; }

    public bool IsForeignKey => ReferencedTable is not null;

    public string? ReferencedDisplayName =>
        ReferencedTable is null ? null : TableInfo.ToDisplayName(ReferencedSchema!, ReferencedTable);

    /// <summary>True when the user may supply a value for this column on insert or update.</summary>
    public bool IsWritable => !IsIdentity && !IsComputed && SqlType is not ("timestamp" or "rowversion");

    public string TypeDisplay => SqlType switch
    {
        "nvarchar" or "nchar" => $"{SqlType}({(MaxLength == -1 ? "max" : MaxLength / 2)})",
        "varchar" or "char" or "varbinary" or "binary" => $"{SqlType}({(MaxLength == -1 ? "max" : MaxLength)})",
        "decimal" or "numeric" => $"{SqlType}({Precision},{Scale})",
        _ => SqlType
    };

    /// <summary>Maximum number of characters for a sized string column; null otherwise.</summary>
    public int? MaxCharacters => (SqlType, MaxLength) switch
    {
        (_, -1) => null,
        ("nvarchar" or "nchar", _) => MaxLength / 2,
        ("varchar" or "char", _) => MaxLength,
        _ => null
    };

    /// <summary>Converts console input into a .NET value of the right type for this column.</summary>
    /// <exception cref="FormatException">The input is not valid for the column's type or size.</exception>
    public object ParseValue(string input)
    {
        var text = input.Trim();
        try
        {
            switch (SqlType)
            {
                case "int": return int.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
                case "bigint": return long.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
                case "smallint": return short.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
                case "tinyint": return byte.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
                case "decimal" or "numeric" or "money" or "smallmoney":
                    return decimal.Parse(text.TrimStart('$'), NumberStyles.Number, CultureInfo.InvariantCulture);
                case "float": return double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
                case "real": return float.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
                case "bit":
                    return text.ToLowerInvariant() switch
                    {
                        "1" or "true" or "yes" or "y" => true,
                        "0" or "false" or "no" or "n" => false,
                        _ => throw new FormatException()
                    };
                case "date" or "datetime" or "datetime2" or "smalldatetime":
                    return DateTime.Parse(text, CultureInfo.CurrentCulture);
                case "time": return TimeSpan.Parse(text, CultureInfo.InvariantCulture);
                case "uniqueidentifier": return Guid.Parse(text);
            }
        }
        catch (Exception ex) when (ex is FormatException or OverflowException)
        {
            var example = SqlType == "bit" ? " Use true/false, yes/no or 1/0." : "";
            throw new FormatException($"'{text}' is not a valid {TypeDisplay} value.{example}", ex);
        }

        if (MaxCharacters is { } max && text.Length > max)
            throw new FormatException($"That is {text.Length} characters; {Name} allows at most {max}.");

        return text;
    }
}
