using System.Windows;
using System.Windows.Controls;
using VetClinic.Data;

namespace VetClinic.App.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => ShowSettings(App.Settings);

    private void ShowSettings(AppSettings settings)
    {
        ThemeSystem.IsChecked = settings.Theme == AppTheme.System;
        ThemeLight.IsChecked = settings.Theme == AppTheme.Light;
        ThemeDark.IsChecked = settings.Theme == AppTheme.Dark;
        FontSizeSlider.Value = settings.FontSize;
        ConfirmDeletesBox.IsChecked = settings.ConfirmDeletes;
        AdminModeBox.IsChecked = settings.AdminMode;
        ConnectionBox.Text = settings.ConnectionString;
        SavedNote.Text = "";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ConnectionBox.Text))
        {
            App.ShowValidation("The connection string cannot be empty.");
            return;
        }

        var settings = App.Settings.Clone();
        settings.Theme = ThemeDark.IsChecked == true ? AppTheme.Dark
            : ThemeLight.IsChecked == true ? AppTheme.Light
            : AppTheme.System;
        settings.FontSize = FontSizeSlider.Value;
        settings.ConfirmDeletes = ConfirmDeletesBox.IsChecked == true;
        settings.AdminMode = AdminModeBox.IsChecked == true;
        settings.ConnectionString = ConnectionBox.Text.Trim();

        try
        {
            App.ApplySettings(settings);
        }
        catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException)
        {
            App.ShowError($"Settings were applied but could not be saved: {ex.Message}");
            return;
        }

        SavedNote.Text = "Settings saved.";
        App.Status("Settings saved.");
    }

    private void Undo_Click(object sender, RoutedEventArgs e) => ShowSettings(App.Settings);

    private void DefaultConnection_Click(object sender, RoutedEventArgs e) =>
        ConnectionBox.Text = SqlDatabase.DefaultConnectionString;

    private void TestConnection_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            new SqlDatabase(ConnectionBox.Text.Trim()).TestConnection();
            MessageBox.Show("Connected successfully.", "Test connection", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) when (ex is DataLayerException or ArgumentException)
        {
            App.ShowError($"Could not connect.\n\n{ex.Message}");
        }
    }
}
