using System.Globalization;
using System.Text.Json;

namespace HotelReservations.Web.Infrastructure;

/// <summary>Per-browser preferences, saved as JSON in a cookie (there are no user accounts).</summary>
public class UserSettings
{
    public const string CookieName = "HotelReservations.Settings";

    public static readonly string[] Themes = ["light", "dark", "auto"];
    public static readonly string[] Accents = ["primary", "success", "danger", "warning", "info", "dark"];
    public static readonly Dictionary<string, string> Languages = new()
    {
        ["en"] = "English",
        ["es"] = "Español",
    };
    public static readonly Dictionary<string, string> DateFormats = new()
    {
        ["MM/dd/yyyy"] = "US (10/19/2026)",
        ["dd/MM/yyyy"] = "European (19/10/2026)",
        ["yyyy-MM-dd"] = "ISO (2026-10-19)",
        ["ddd, MMM d, yyyy"] = "Long (Mon, Oct 19, 2026)",
    };

    public string Theme { get; set; } = "light";
    public string Accent { get; set; } = "primary";
    public string Language { get; set; } = "en";
    public string DateFormat { get; set; } = "MM/dd/yyyy";
    public bool ShowCancelled { get; set; } = true;
    public bool ConfirmDeletes { get; set; } = true;

    /// <summary>Unlocks the Administration page (no password: the assignment asks for no security).</summary>
    public bool AdminMode { get; set; }

    /// <summary>Translates a piece of interface text into the chosen language.</summary>
    public string T(string english) => Translations.Get(Language, english);

    public string FormatDate(DateOnly date) =>
        date.ToString(DateFormat, Language == "es" ? CultureInfo.GetCultureInfo("es-ES") : CultureInfo.InvariantCulture);

    /// <summary>Replaces any unexpected value (e.g. from an edited cookie) with the default.</summary>
    public UserSettings Normalize()
    {
        var defaults = new UserSettings();
        if (!Themes.Contains(Theme)) Theme = defaults.Theme;
        if (!Accents.Contains(Accent)) Accent = defaults.Accent;
        if (!Languages.ContainsKey(Language)) Language = defaults.Language;
        if (!DateFormats.ContainsKey(DateFormat)) DateFormat = defaults.DateFormat;
        return this;
    }

    public static UserSettings Load(HttpRequest request)
    {
        if (request.Cookies.TryGetValue(CookieName, out var json))
        {
            try
            {
                return (JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings()).Normalize();
            }
            catch (JsonException)
            {
                // Corrupt cookie: fall back to defaults.
            }
        }
        return new UserSettings();
    }

    public void Save(HttpResponse response) =>
        response.Cookies.Append(CookieName, JsonSerializer.Serialize(Normalize()), new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
        });
}
