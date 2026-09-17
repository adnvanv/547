using System.Data;
using LibraryManager.Data;
using Microsoft.Data.SqlClient;

namespace LibraryManager;

/// <summary>
/// Console user interface: shows the database summary, then runs the
/// Get / Add / Update / Delete workflows until the user quits.
/// </summary>
internal sealed class LibraryApp
{
    private const int MaxLookupRows = 25;

    private readonly DatabaseRepository _repository;

    public LibraryApp(DatabaseRepository repository) => _repository = repository;

    public int Run()
    {
        ConsoleUi.WriteBanner("Library Manager - SQL Server Express console client");

        try
        {
            ShowSummary();
        }
        catch (SqlException ex)
        {
            ConsoleUi.WriteError($"Could not read the database: {ex.Message}");
            Console.WriteLine("Check that SQL Server Express is running and that Scripts/01_LibraryDB_DDL.sql and");
            Console.WriteLine("Scripts/02_LibraryDB_DML.sql have been executed. A different connection string can be");
            Console.WriteLine("passed as the first command-line argument.");
            return 1;
        }

        try
        {
            while (true)
            {
                Console.WriteLine();
                ConsoleUi.WriteHeader("Main Menu");
                Console.WriteLine("  1) Get data      - list every record in a table");
                Console.WriteLine("  2) Add data      - insert a new record");
                Console.WriteLine("  3) Update data   - change one column of a record");
                Console.WriteLine("  4) Delete data   - remove a record");
                Console.WriteLine("  5) Show database summary");
                Console.WriteLine("  Q) Quit");

                var choice = ConsoleUi.Prompt("Choose an option").ToUpperInvariant();
                if (choice is "Q" or "QUIT" or "EXIT")
                    break;

                try
                {
                    switch (choice)
                    {
                        case "1": GetData(); break;
                        case "2": AddData(); break;
                        case "3": UpdateData(); break;
                        case "4": DeleteData(); break;
                        case "5": ShowSummary(); break;
                        default: ConsoleUi.WriteError("Please enter a number from 1 to 5, or Q to quit."); break;
                    }
                }
                catch (SqlException ex)
                {
                    ConsoleUi.WriteError(DescribeSqlError(ex));
                }
            }
        }
        catch (InputClosedException)
        {
            // Standard input was closed (e.g. piped input ran out) - treat it like Quit.
        }

        Console.WriteLine();
        Console.WriteLine("Goodbye!");
        return 0;
    }

    // ---------------------------------------------------------------- Summary

    private void ShowSummary()
    {
        var server = _repository.GetServerInfo();
        var tables = _repository.GetTables();

        Console.WriteLine();
        ConsoleUi.WriteHeader("Database Summary");
        Console.WriteLine($"  Database      : {server.DatabaseName}");
        Console.WriteLine($"  Server        : {server.ServerName} ({server.Edition}, version {server.Version})");
        Console.WriteLine($"  Tables        : {tables.Count} ({string.Join(", ", tables.Select(t => t.DisplayName))})");
        Console.WriteLine($"  Total records : {tables.Sum(t => t.RowCount)}");

        foreach (var table in tables)
        {
            Console.WriteLine();
            ConsoleUi.WriteSubheader($"{table.DisplayName} - {table.RowCount} record(s)");
            if (!string.IsNullOrWhiteSpace(table.Description))
                Console.WriteLine($"  {table.Description}");

            var grid = new DataTable();
            foreach (var heading in new[] { "Column", "Type", "Nullable", "Key / Notes", "Description" })
                grid.Columns.Add(heading);
            foreach (var column in table.Columns)
                grid.Rows.Add(column.Name, column.TypeDisplay, column.IsNullable ? "yes" : "no",
                    DescribeColumn(column), column.Description ?? "");

            ConsoleUi.PrintTable(grid, indent: 2, maxColumnWidth: 45);
        }

        var relationships = tables
            .SelectMany(t => t.Columns.Where(c => c.IsForeignKey)
                .Select(c => $"{t.DisplayName}.{c.Name} -> {c.ReferencedDisplayName}.{c.ReferencedColumn}"))
            .ToList();
        if (relationships.Count > 0)
        {
            Console.WriteLine();
            ConsoleUi.WriteSubheader("Relationships (foreign keys)");
            foreach (var relationship in relationships)
                Console.WriteLine($"  {relationship}");
        }
    }

    private static string DescribeColumn(ColumnInfo column)
    {
        var notes = new List<string>();
        if (column.IsPrimaryKey) notes.Add("PK");
        if (column.IsIdentity) notes.Add("identity");
        if (column.IsForeignKey) notes.Add($"FK -> {column.ReferencedDisplayName}.{column.ReferencedColumn}");
        if (column.HasDefault) notes.Add("has default");
        if (column.IsComputed) notes.Add("computed");
        return string.Join(", ", notes);
    }

    // ---------------------------------------------------------------- Get

    private void GetData()
    {
        var table = SelectTable("Get Data");
        if (table is null)
            return;

        ShowAllRows(table);
    }

    // ---------------------------------------------------------------- Add

    private void AddData()
    {
        var table = SelectTable("Add Data");
        if (table is null || GetPrimaryKeyOrWarn(table) is not { } primaryKey)
            return;

        Console.WriteLine();
        Console.WriteLine($"Enter a value for each column of the new {table.DisplayName} record.");
        Console.WriteLine("Blank = column default (or NULL if optional); type NULL to leave an optional column empty.");

        var values = new List<(ColumnInfo Column, object? Value)>();
        foreach (var column in table.Columns)
        {
            if (!column.IsWritable)
            {
                Console.WriteLine($"  {column.Name}: generated by the database");
                continue;
            }

            if (column.IsForeignKey)
                ShowForeignKeyChoices(column);

            var (value, useDefault) = PromptColumnValue(column, allowDefault: true);
            if (!useDefault)
                values.Add((column, value));
        }

        var newId = _repository.InsertRow(table, values);

        Console.WriteLine();
        ConsoleUi.WriteSuccess($"Record added to {table.DisplayName} with {primaryKey.Name} = {ConsoleUi.FormatValue(newId)}.");
        Console.WriteLine("Reading it back from the database:");
        ConsoleUi.PrintTable(_repository.GetRowById(table, newId));
        Console.WriteLine($"{table.DisplayName} now has {_repository.CountRows(table)} record(s).");
    }

    // ---------------------------------------------------------------- Update

    private void UpdateData()
    {
        var table = SelectTable("Update Data");
        if (table is null || GetPrimaryKeyOrWarn(table) is not { } primaryKey)
            return;

        ShowAllRows(table);

        var id = PromptExistingId(table, primaryKey, "update");
        if (id is null)
            return;

        var editable = table.Columns.Where(c => c.IsWritable && !c.IsPrimaryKey).ToList();
        if (editable.Count == 0)
        {
            ConsoleUi.WriteError($"{table.DisplayName} has no columns that can be updated.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Columns that can be updated:");
        for (var i = 0; i < editable.Count; i++)
            Console.WriteLine($"  {i + 1}) {editable[i].Name} ({editable[i].TypeDisplay})");

        var column = PromptChoice(editable, c => c.Name, "Pick a column (number or name, blank to cancel)");
        if (column is null)
            return;

        var before = _repository.GetRowById(table, id);
        Console.WriteLine($"Current {column.Name}: {ConsoleUi.FormatValue(before.Rows[0][column.Name])}");

        if (column.IsForeignKey)
            ShowForeignKeyChoices(column);

        var (newValue, _) = PromptColumnValue(column, allowDefault: false);
        var affected = _repository.UpdateColumn(table, id, column, newValue);

        var after = _repository.GetRowById(table, id);
        Console.WriteLine();
        ConsoleUi.WriteSuccess($"{affected} record updated: {column.Name} changed from " +
                               $"{ConsoleUi.FormatValue(before.Rows[0][column.Name])} to {ConsoleUi.FormatValue(after.Rows[0][column.Name])}.");
        Console.WriteLine("Before:");
        ConsoleUi.PrintTable(before);
        Console.WriteLine("After (read back from the database):");
        ConsoleUi.PrintTable(after);
    }

    // ---------------------------------------------------------------- Delete

    private void DeleteData()
    {
        var table = SelectTable("Delete Data");
        if (table is null || GetPrimaryKeyOrWarn(table) is not { } primaryKey)
            return;

        ShowAllRows(table);

        var id = PromptExistingId(table, primaryKey, "delete");
        if (id is null)
            return;

        Console.WriteLine("Record to delete:");
        ConsoleUi.PrintTable(_repository.GetRowById(table, id));
        if (!ConsoleUi.Confirm("Delete this record?"))
        {
            Console.WriteLine("Delete cancelled.");
            return;
        }

        var affected = _repository.DeleteRow(table, id);
        Console.WriteLine();
        ConsoleUi.WriteSuccess($"{affected} record deleted from {table.DisplayName}.");

        var stillThere = _repository.GetRowById(table, id).Rows.Count > 0;
        if (stillThere)
            ConsoleUi.WriteError($"Unexpected: {primaryKey.Name} = {ConsoleUi.FormatValue(id)} is still in the table.");
        else
            Console.WriteLine($"Verified: no {table.DisplayName} record with {primaryKey.Name} = {ConsoleUi.FormatValue(id)} exists anymore.");

        ShowAllRows(table);
    }

    // ---------------------------------------------------------------- Shared prompts

    private TableInfo? SelectTable(string title)
    {
        var tables = _repository.GetTables();
        Console.WriteLine();
        ConsoleUi.WriteHeader(title);

        if (tables.Count == 0)
        {
            ConsoleUi.WriteError("The database has no tables.");
            return null;
        }

        for (var i = 0; i < tables.Count; i++)
            Console.WriteLine($"  {i + 1}) {tables[i].DisplayName} ({tables[i].RowCount} records)");

        return PromptChoice(tables, t => t.DisplayName, "Pick a table (number or name, blank to go back)");
    }

    private void ShowAllRows(TableInfo table)
    {
        var rows = _repository.GetAllRows(table);
        Console.WriteLine();
        ConsoleUi.WriteSubheader($"{table.DisplayName} - {rows.Rows.Count} record(s)");
        ConsoleUi.PrintTable(rows);
    }

    private void ShowForeignKeyChoices(ColumnInfo column)
    {
        var rows = _repository.GetReferencedRows(column, MaxLookupRows);
        Console.WriteLine($"  {column.Name} must match an existing {column.ReferencedDisplayName}.{column.ReferencedColumn}:");
        ConsoleUi.PrintTable(rows, indent: 4);
        if (rows.Rows.Count == MaxLookupRows)
            Console.WriteLine($"    (showing the first {MaxLookupRows})");
    }

    /// <summary>Asks for a primary key value until it matches an existing record. Returns null if cancelled.</summary>
    private object? PromptExistingId(TableInfo table, ColumnInfo primaryKey, string verb)
    {
        while (true)
        {
            var input = ConsoleUi.Prompt($"Enter the {primaryKey.Name} of the record to {verb} (blank to cancel)");
            if (input.Length == 0)
                return null;

            object id;
            try
            {
                id = primaryKey.ParseValue(input);
            }
            catch (FormatException ex)
            {
                ConsoleUi.WriteError(ex.Message);
                continue;
            }

            if (_repository.GetRowById(table, id).Rows.Count > 0)
                return id;

            ConsoleUi.WriteError($"No {table.DisplayName} record has {primaryKey.Name} = {input}.");
        }
    }

    /// <summary>
    /// Asks for a column value until it is valid for the column's type.
    /// UseDefault is true when the user left it blank and the column has a default (insert only).
    /// </summary>
    private static (object? Value, bool UseDefault) PromptColumnValue(ColumnInfo column, bool allowDefault)
    {
        var blankMeansDefault = allowDefault && column.HasDefault;
        var hint = blankMeansDefault ? "blank = default" : column.IsNullable ? "optional" : "required";

        while (true)
        {
            var input = ConsoleUi.Prompt($"  {column.Name} [{column.TypeDisplay}, {hint}]");

            if (input.Length == 0)
            {
                if (blankMeansDefault)
                    return (null, true);
                if (column.IsNullable)
                    return (null, false);
                ConsoleUi.WriteError($"  {column.Name} is required.");
                continue;
            }

            if (input.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            {
                if (column.IsNullable)
                    return (null, false);
                ConsoleUi.WriteError($"  {column.Name} does not allow NULL.");
                continue;
            }

            try
            {
                return (column.ParseValue(input), false);
            }
            catch (FormatException ex)
            {
                ConsoleUi.WriteError($"  {ex.Message}");
            }
        }
    }

    /// <summary>Lets the user pick an item by its 1-based number or its name. Returns null on blank input.</summary>
    private static T? PromptChoice<T>(IReadOnlyList<T> items, Func<T, string> getName, string prompt) where T : class
    {
        while (true)
        {
            var input = ConsoleUi.Prompt(prompt);
            if (input.Length == 0)
                return null;

            if (int.TryParse(input, out var number) && number >= 1 && number <= items.Count)
                return items[number - 1];

            var match = items.FirstOrDefault(item => string.Equals(getName(item), input, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
                return match;

            ConsoleUi.WriteError($"'{input}' is not one of the choices listed above.");
        }
    }

    private static ColumnInfo? GetPrimaryKeyOrWarn(TableInfo table)
    {
        if (table.PrimaryKey is null)
            ConsoleUi.WriteError($"{table.DisplayName} has no single-column primary key, so its records can't be picked by ID.");
        return table.PrimaryKey;
    }

    private static string DescribeSqlError(SqlException ex) => ex.Number switch
    {
        547 => "The change was rejected by a constraint, so nothing was saved. This usually means a foreign key\n" +
               "(e.g. deleting a record that other records still reference) or a CHECK rule (e.g. a negative price).\n" +
               $"SQL Server: {ex.Message}",
        2601 or 2627 => $"That would create a duplicate key, so nothing was saved.\nSQL Server: {ex.Message}",
        8115 => $"A number is too large for its column, so nothing was saved.\nSQL Server: {ex.Message}",
        _ => $"Database error {ex.Number}: {ex.Message}"
    };
}
