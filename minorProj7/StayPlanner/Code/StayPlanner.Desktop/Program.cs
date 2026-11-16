using Avalonia;

namespace StayPlanner.Desktop;

internal static class Program
{
    // Avalonia picks the windowing backend for the current OS: Win32 on Windows, X11 on Linux, Cocoa on macOS.
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    // Also used by the XAML previewer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
