using System.IO;
using System.Text.Json;
using VetClinic.Data;

namespace VetClinic.App;

public enum AppTheme
{
    System,
    Light,
    Dark,
}

/// <summary>User settings, saved as JSON in %APPDATA%\VetClinic\settings.json.</summary>
public sealed class AppSettings
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VetClinic", "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public AppTheme Theme { get; set; } = AppTheme.System;
    public double FontSize { get; set; } = 14;
    public bool ConfirmDeletes { get; set; } = true;
    public bool AdminMode { get; set; }
    public string ConnectionString { get; set; } = SqlDatabase.DefaultConnectionString;

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath), JsonOptions) ?? new();
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // A corrupt or unreadable settings file falls back to the defaults.
        }
        return new AppSettings();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this, JsonOptions));
    }

    public AppSettings Clone() => (AppSettings)MemberwiseClone();
}
