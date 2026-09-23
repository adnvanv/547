using System.Windows;
using System.Windows.Controls;
using VetClinic.Data;
using static VetClinic.App.Views.ViewHelpers;

namespace VetClinic.App.Views;

/// <summary>Administrator-only page for the Veterinarians table.</summary>
public partial class AdminView : UserControl
{
    private Veterinarian? _current;

    public AdminView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var unlocked = App.Settings.AdminMode;
        LockedPanel.Visibility = unlocked ? Visibility.Collapsed : Visibility.Visible;
        AdminPanel.Visibility = unlocked ? Visibility.Visible : Visibility.Collapsed;
        if (unlocked)
            LoadVets(_current?.VeterinarianId);
    }

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is MainWindow { Tabs: var tabs })
            tabs.SelectedIndex = tabs.Items.Count - 1;
    }

    private void LoadVets(int? selectId = null)
    {
        List<Veterinarian> vets = [];
        if (!TryData(() => vets = App.Veterinarians.GetAll()))
            return;

        Grid.ItemsSource = vets;
        ResultCount.Text = $"{vets.Count} veterinarian(s), {vets.Count(v => v.IsActive)} active";

        var match = vets.FirstOrDefault(v => v.VeterinarianId == selectId);
        if (match is not null)
        {
            Grid.SelectedItem = match;
            ShowVet(match); // selection may not change if it is the same row
        }
        else
        {
            ShowVet(null);
        }
    }

    private void ShowVet(Veterinarian? vet)
    {
        _current = vet;
        FormTitle.Text = vet is null ? "New veterinarian" : $"Edit veterinarian #{vet.VeterinarianId}";
        IdBox.Text = vet?.VeterinarianId.ToString() ?? "(assigned on save)";
        FirstNameBox.Text = vet?.FirstName ?? "";
        LastNameBox.Text = vet?.LastName ?? "";
        SpecialtyBox.Text = vet?.Specialty ?? "";
        ActiveBox.IsChecked = vet?.IsActive ?? true;
        DeleteButton.IsEnabled = vet is not null;
    }

    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Grid.SelectedItem is Veterinarian vet)
            ShowVet(vet);
    }

    private void New_Click(object sender, RoutedEventArgs e)
    {
        Grid.SelectedItem = null;
        ShowVet(null);
        FirstNameBox.Focus();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var vet = new Veterinarian
        {
            VeterinarianId = _current?.VeterinarianId ?? 0,
            FirstName = FirstNameBox.Text.Trim(),
            LastName = LastNameBox.Text.Trim(),
            Specialty = Blank(SpecialtyBox.Text),
            IsActive = ActiveBox.IsChecked == true,
        };
        if (vet.FirstName.Length == 0 || vet.LastName.Length == 0)
        {
            App.ShowValidation("First name and last name are required.");
            return;
        }

        var isNew = _current is null;
        if (!TryData(() => { if (isNew) App.Veterinarians.Insert(vet); else App.Veterinarians.Update(vet); }))
            return;

        App.Status(isNew ? $"Added veterinarian #{vet.VeterinarianId}." : $"Updated veterinarian #{vet.VeterinarianId}.");
        LoadVets(vet.VeterinarianId);
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (_current is not { } vet)
            return;
        if (vet.AppointmentCount > 0)
        {
            App.ShowValidation($"{vet} has {vet.AppointmentCount} appointment(s) on record and cannot be deleted. " +
                               "Clear the Active box and save to retire them instead.");
            return;
        }
        if (!App.ConfirmDelete($"Delete veterinarian #{vet.VeterinarianId} {vet}?"))
            return;
        if (!TryData(() => App.Veterinarians.Delete(vet.VeterinarianId)))
            return;

        App.Status($"Deleted veterinarian #{vet.VeterinarianId}.");
        LoadVets();
    }
}
