using System.Text;
using LibraryManager.Data;

namespace LibraryManager;

internal static class Program
{
    // Windows authentication against the local SQL Server Express instance.
    private const string DefaultConnectionString =
        @"Server=.\SQLEXPRESS;Database=LibraryDB;Integrated Security=True;TrustServerCertificate=True;";

    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Connection string precedence: first command-line argument, LIBRARYDB_CONNECTION env var, default.
        var connectionString = args.Length > 0
            ? args[0]
            : Environment.GetEnvironmentVariable("LIBRARYDB_CONNECTION") ?? DefaultConnectionString;

        return new LibraryApp(new DatabaseRepository(connectionString)).Run();
    }
}
