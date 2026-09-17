using System.Data;
using System.Globalization;

namespace LibraryManager;

/// <summary>Thrown when standard input is closed while the app is waiting for the user.</summary>
internal sealed class InputClosedException : Exception;

/// <summary>Console input/output helpers: prompts, colored messages and text tables.</summary>
internal static class ConsoleUi
{
    private const int DefaultMaxColumnWidth = 32;

    public static string Prompt(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"{message}: ");
        Console.ResetColor();
        var line = Console.ReadLine() ?? throw new InputClosedException();
        // Piped input can start with a byte-order mark, which Trim() does not remove.
        return line.Trim().Trim('﻿');
    }

    public static bool Confirm(string message)
    {
        while (true)
        {
            var answer = Prompt($"{message} (y/n)").ToLowerInvariant();
            if (answer is "y" or "yes")
                return true;
            if (answer is "n" or "no")
                return false;
            WriteError("Please answer y or n.");
        }
    }

    public static void WriteBanner(string title)
    {
        var rule = new string('=', title.Length + 4);
        WriteColored(rule, ConsoleColor.Yellow);
        WriteColored($"  {title}", ConsoleColor.Yellow);
        WriteColored(rule, ConsoleColor.Yellow);
    }

    public static void WriteHeader(string text) => WriteColored($"=== {text} ===", ConsoleColor.Yellow);

    public static void WriteSubheader(string text) => WriteColored($"--- {text} ---", ConsoleColor.DarkYellow);

    public static void WriteSuccess(string text) => WriteColored(text, ConsoleColor.Green);

    public static void WriteError(string text) => WriteColored(text, ConsoleColor.Red);

    private static void WriteColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static string FormatValue(object? value) => value switch
    {
        null or DBNull => "NULL",
        bool b => b ? "true" : "false",
        DateTime d => d.TimeOfDay == TimeSpan.Zero ? d.ToString("yyyy-MM-dd") : d.ToString("yyyy-MM-dd HH:mm:ss"),
        string s => s.ReplaceLineEndings(" "),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? ""
    };

    /// <summary>Prints a DataTable as aligned text columns; numbers are right-aligned, long values truncated.</summary>
    public static void PrintTable(DataTable table, int indent = 0, int maxColumnWidth = DefaultMaxColumnWidth)
    {
        var pad = new string(' ', indent);
        if (table.Rows.Count == 0)
        {
            Console.WriteLine($"{pad}(no records)");
            return;
        }

        var columns = table.Columns.Cast<DataColumn>().ToArray();
        var headers = columns.Select(c => Truncate(c.ColumnName, maxColumnWidth)).ToArray();
        var rows = table.Rows.Cast<DataRow>()
            .Select(row => columns.Select(c => Truncate(FormatValue(row[c]), maxColumnWidth)).ToArray())
            .ToList();
        var widths = headers.Select((header, i) => Math.Max(header.Length, rows.Max(r => r[i].Length))).ToArray();
        var rightAlign = columns.Select(c => IsNumeric(c.DataType)).ToArray();

        string FormatLine(string[] cells) =>
            (pad + string.Join(" | ", cells.Select((cell, i) => rightAlign[i] ? cell.PadLeft(widths[i]) : cell.PadRight(widths[i])))).TrimEnd();

        Console.WriteLine(FormatLine(headers));
        Console.WriteLine(pad + string.Join("-+-", widths.Select(w => new string('-', w))));
        foreach (var row in rows)
            Console.WriteLine(FormatLine(row));
    }

    private static string Truncate(string text, int maxLength) =>
        text.Length <= maxLength ? text : text[..(maxLength - 3)] + "...";

    private static bool IsNumeric(Type type) => Type.GetTypeCode(type) is
        TypeCode.Byte or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or
        TypeCode.Decimal or TypeCode.Double or TypeCode.Single;
}
