using System.Windows;
using System.Windows.Controls;
using VetClinic.Data;

namespace VetClinic.App;

public partial class App : Application
{
    public static AppSettings Settings { get; private set; } = new();
    public static OwnerRepository Owners { get; private set; } = null!;
    public static PetRepository Pets { get; private set; } = null!;
    public static AppointmentRepository Appointments { get; private set; } = null!;
    public static VeterinarianRepository Veterinarians { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show(args.Exception.Message, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        AddControlStyles();
        Settings = AppSettings.Load();
        Connect(Settings.ConnectionString);
        ApplyAppearance(Settings);

        MainWindow = new MainWindow();
        MainWindow.Show();
    }

    /// <summary>
    /// Adds app-wide implicit styles on top of the Fluent ones. Built in code because
    /// FindResource returns the Fluent base style, while a XAML StaticResource BasedOn does not.
    /// </summary>
    private void AddControlStyles()
    {
        void Add(Type type, params (DependencyProperty Property, object Value)[] setters)
        {
            var style = new Style(type, (Style)FindResource(type));
            foreach (var (property, value) in setters)
                style.Setters.Add(new Setter(property, value));
            Resources[type] = style;
        }

        var fieldMargin = new Thickness(0, 0, 0, 8);
        Add(typeof(TextBox), (FrameworkElement.MarginProperty, fieldMargin));
        Add(typeof(ComboBox), (FrameworkElement.MarginProperty, fieldMargin));
        Add(typeof(DatePicker), (FrameworkElement.MarginProperty, fieldMargin));
        Add(typeof(Button),
            (FrameworkElement.MarginProperty, new Thickness(0, 0, 8, 0)),
            (FrameworkElement.MinWidthProperty, 90.0));
        Add(typeof(DataGrid),
            (DataGrid.AutoGenerateColumnsProperty, false),
            (DataGrid.IsReadOnlyProperty, true),
            (DataGrid.SelectionModeProperty, DataGridSelectionMode.Single),
            (DataGrid.CanUserAddRowsProperty, false),
            (DataGrid.CanUserDeleteRowsProperty, false),
            (DataGrid.HeadersVisibilityProperty, DataGridHeadersVisibility.Column),
            (DataGrid.GridLinesVisibilityProperty, DataGridGridLinesVisibility.Horizontal));
    }

    public static void Connect(string connectionString)
    {
        var db = new SqlDatabase(connectionString);
        Owners = new OwnerRepository(db);
        Pets = new PetRepository(db);
        Appointments = new AppointmentRepository(db);
        Veterinarians = new VeterinarianRepository(db);
    }

    public static void ApplySettings(AppSettings settings)
    {
        var reconnect = settings.ConnectionString != Settings.ConnectionString;
        Settings = settings;
        Settings.Save();
        if (reconnect)
            Connect(settings.ConnectionString);
        ApplyAppearance(settings);
    }

    /// <summary>Applies the theme (WPF Fluent light/dark) and the base font size.</summary>
    public static void ApplyAppearance(AppSettings settings)
    {
#pragma warning disable WPF0001 // ThemeMode is marked experimental
        Current.ThemeMode = settings.Theme switch
        {
            AppTheme.Light => ThemeMode.Light,
            AppTheme.Dark => ThemeMode.Dark,
            _ => ThemeMode.System,
        };
#pragma warning restore WPF0001

        // Fluent control templates read their text size from this resource.
        Current.Resources["ControlContentThemeFontSize"] = settings.FontSize;
        foreach (Window window in Current.Windows)
            window.FontSize = settings.FontSize;
    }

    /// <summary>Asks for confirmation (unless disabled in Settings) before a delete.</summary>
    public static bool ConfirmDelete(string message) =>
        !Settings.ConfirmDeletes ||
        MessageBox.Show(message, "Confirm delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)
            == MessageBoxResult.Yes;

    public static void ShowError(string message) =>
        MessageBox.Show(message, "Vet Clinic", MessageBoxButton.OK, MessageBoxImage.Error);

    public static void Status(string message) => (Current.MainWindow as MainWindow)?.SetStatus(message);

    public static void ShowValidation(string message) =>
        MessageBox.Show(message, "Please check your input", MessageBoxButton.OK, MessageBoxImage.Information);
}
