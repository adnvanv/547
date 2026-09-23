using VetClinic.Data;

namespace VetClinic.App.Views;

// Each view reloads its data in its Loaded handler. A TabControl re-attaches tab content
// every time the tab is shown, so edits made on other tabs (a new owner, a renamed vet, ...)
// always appear in that tab's lists and drop-downs.
internal static class ViewHelpers
{
    /// <summary>Runs a data-layer call, showing database errors to the user instead of crashing.</summary>
    public static bool TryData(Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (DataLayerException ex)
        {
            App.ShowError(ex.Message);
            return false;
        }
    }

    public static string? Blank(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
