using System.Text.Json;

namespace StayPlanner.Data;

public interface IRoomRepository
{
    IReadOnlyList<Room> SelectAll();
    Room? SelectById(int id);
}

public interface IReservationRepository
{
    string StoragePath { get; }
    IReadOnlyList<Reservation> SelectAll();
    Reservation Insert(Reservation reservation);
    bool Delete(int id);
}

internal static class Json
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { WriteIndented = true };
}

/// <summary>Reads rooms from rooms.json, compiled into the assembly so it ships on every OS.</summary>
public sealed class EmbeddedRoomRepository : IRoomRepository
{
    private readonly List<Room> _rooms;

    public EmbeddedRoomRepository()
    {
        var assembly = typeof(EmbeddedRoomRepository).Assembly;
        var name = assembly.GetManifestResourceNames().Single(n => n.EndsWith("rooms.json", StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(name)!;
        _rooms = JsonSerializer.Deserialize<List<Room>>(stream, Json.Options) ?? [];
    }

    public IReadOnlyList<Room> SelectAll() => _rooms;

    public Room? SelectById(int id) => _rooms.FirstOrDefault(r => r.Id == id);
}

/// <summary>
/// Stores reservations as JSON in the per-user application data folder. That folder is
/// %APPDATA% on Windows, ~/.config on Linux and ~/Library/Application Support on macOS;
/// Path.Combine picks the right directory separator for each.
/// </summary>
public sealed class JsonReservationRepository : IReservationRepository
{
    private readonly object _lock = new();

    public JsonReservationRepository(string? storagePath = null)
    {
        StoragePath = storagePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create),
            "StayPlanner",
            "reservations.json");
    }

    public string StoragePath { get; }

    public IReadOnlyList<Reservation> SelectAll()
    {
        lock (_lock) return Load();
    }

    public Reservation Insert(Reservation reservation)
    {
        lock (_lock)
        {
            var all = Load();
            var saved = reservation with { Id = all.Count == 0 ? 1 : all.Max(r => r.Id) + 1 };
            all.Add(saved);
            Save(all);
            return saved;
        }
    }

    public bool Delete(int id)
    {
        lock (_lock)
        {
            var all = Load();
            var removed = all.RemoveAll(r => r.Id == id) > 0;
            if (removed) Save(all);
            return removed;
        }
    }

    private List<Reservation> Load()
    {
        if (!File.Exists(StoragePath)) return [];
        var text = File.ReadAllText(StoragePath);
        return string.IsNullOrWhiteSpace(text) ? [] : JsonSerializer.Deserialize<List<Reservation>>(text, Json.Options) ?? [];
    }

    private void Save(List<Reservation> all)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(StoragePath)!);
        var temp = StoragePath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(all, Json.Options));
        File.Move(temp, StoragePath, overwrite: true);
    }
}
