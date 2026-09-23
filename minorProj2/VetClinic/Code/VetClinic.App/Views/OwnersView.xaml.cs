using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VetClinic.Data;
using static VetClinic.App.Views.ViewHelpers;

namespace VetClinic.App.Views;

public partial class OwnersView : UserControl
{
    private static readonly (OwnerSearchField Field, string Label)[] SearchFields =
    [
        (OwnerSearchField.AnyField, "Any field"),
        (OwnerSearchField.Id, "ID"),
        (OwnerSearchField.FirstName, "First name"),
        (OwnerSearchField.LastName, "Last name"),
        (OwnerSearchField.Email, "Email"),
        (OwnerSearchField.Phone, "Phone"),
        (OwnerSearchField.City, "City"),
    ];

    private Owner? _current;

    public OwnersView()
    {
        InitializeComponent();
        SearchField.ItemsSource = SearchFields.Select(f => f.Label).ToList();
        SearchField.SelectedIndex = 0;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => LoadOwners(_current?.OwnerId);

    private void LoadOwners(int? selectId = null)
    {
        var field = SearchFields[Math.Max(SearchField.SelectedIndex, 0)].Field;
        List<Owner> owners = [];
        if (!TryData(() => owners = App.Owners.Search(field, SearchBox.Text)))
            return;

        Grid.ItemsSource = owners;
        ResultCount.Text = string.IsNullOrWhiteSpace(SearchBox.Text)
            ? $"{owners.Count} owner(s)"
            : $"{owners.Count} owner(s) matching \"{SearchBox.Text.Trim()}\" in {SearchFields[SearchField.SelectedIndex].Label}";

        var match = owners.FirstOrDefault(o => o.OwnerId == selectId);
        if (match is not null)
        {
            Grid.SelectedItem = match;
            ShowOwner(match); // selection may not change if it is the same row
        }
        else
        {
            ShowOwner(null);
        }
    }

    private void ShowOwner(Owner? owner)
    {
        _current = owner;
        FormTitle.Text = owner is null ? "New owner" : $"Edit owner #{owner.OwnerId}";
        IdBox.Text = owner?.OwnerId.ToString() ?? "(assigned on save)";
        FirstNameBox.Text = owner?.FirstName ?? "";
        LastNameBox.Text = owner?.LastName ?? "";
        EmailBox.Text = owner?.Email ?? "";
        PhoneBox.Text = owner?.Phone ?? "";
        CityBox.Text = owner?.City ?? "";
        DeleteButton.IsEnabled = owner is not null;
    }

    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Grid.SelectedItem is Owner owner)
            ShowOwner(owner);
    }

    private void Search_Click(object sender, RoutedEventArgs e) => LoadOwners();

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            LoadOwners();
    }

    private void ClearSearch_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.Clear();
        SearchField.SelectedIndex = 0;
        LoadOwners();
    }

    private void New_Click(object sender, RoutedEventArgs e)
    {
        Grid.SelectedItem = null;
        ShowOwner(null);
        FirstNameBox.Focus();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var owner = new Owner
        {
            OwnerId = _current?.OwnerId ?? 0,
            FirstName = FirstNameBox.Text.Trim(),
            LastName = LastNameBox.Text.Trim(),
            Email = Blank(EmailBox.Text),
            Phone = Blank(PhoneBox.Text),
            City = Blank(CityBox.Text),
        };

        if (owner.FirstName.Length == 0 || owner.LastName.Length == 0)
        {
            App.ShowValidation("First name and last name are required.");
            return;
        }
        if (owner.Email is not null && !Regex.IsMatch(owner.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            App.ShowValidation("Enter a valid email address, e.g. name@example.com.");
            return;
        }
        if (owner.Phone is not null && !Regex.IsMatch(owner.Phone, @"^[0-9()+\-.\s]{7,25}$"))
        {
            App.ShowValidation("A phone number may contain only digits, spaces and ( ) + - .");
            return;
        }

        var isNew = _current is null;
        if (!TryData(() => { if (isNew) App.Owners.Insert(owner); else App.Owners.Update(owner); }))
            return;

        App.Status(isNew ? $"Added owner #{owner.OwnerId} {owner.FullName}." : $"Updated owner #{owner.OwnerId} {owner.FullName}.");
        LoadOwners(owner.OwnerId);
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (_current is not { } owner)
            return;

        var petNote = owner.PetCount > 0 ? $"\n\nThis also deletes their {owner.PetCount} pet(s) and all of those pets' appointments." : "";
        if (!App.ConfirmDelete($"Delete owner #{owner.OwnerId} {owner.FullName}?{petNote}"))
            return;
        if (!TryData(() => App.Owners.Delete(owner.OwnerId)))
            return;

        App.Status($"Deleted owner #{owner.OwnerId} {owner.FullName}.");
        LoadOwners();
    }
}
