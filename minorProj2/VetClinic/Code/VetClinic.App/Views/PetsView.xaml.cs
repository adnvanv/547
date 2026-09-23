using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using VetClinic.Data;
using static VetClinic.App.Views.ViewHelpers;

namespace VetClinic.App.Views;

public partial class PetsView : UserControl
{
    private static readonly string[] CommonSpecies = ["Dog", "Cat", "Bird", "Rabbit", "Hamster", "Guinea Pig", "Reptile", "Fish"];

    private Pet? _current;

    public PetsView()
    {
        InitializeComponent();
        SpeciesBox.ItemsSource = CommonSpecies;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => LoadPets(_current?.PetId);

    private void LoadPets(int? selectId = null)
    {
        List<Pet> pets = [];
        List<Owner> owners = [];
        if (!TryData(() => { pets = App.Pets.GetAll(); owners = App.Owners.GetAll(); }))
            return;

        OwnerBox.ItemsSource = owners;
        Grid.ItemsSource = pets;
        ResultCount.Text = $"{pets.Count} pet(s)";

        var match = pets.FirstOrDefault(p => p.PetId == selectId);
        if (match is not null)
        {
            Grid.SelectedItem = match;
            ShowPet(match); // selection may not change if it is the same row
        }
        else
        {
            ShowPet(null);
        }
    }

    private void ShowPet(Pet? pet)
    {
        _current = pet;
        FormTitle.Text = pet is null ? "New pet" : $"Edit pet #{pet.PetId}";
        IdBox.Text = pet?.PetId.ToString() ?? "(assigned on save)";
        OwnerBox.SelectedValue = pet?.OwnerId;
        NameBox.Text = pet?.Name ?? "";
        SpeciesBox.Text = pet?.Species ?? "";
        BreedBox.Text = pet?.Breed ?? "";
        BirthDateBox.SelectedDate = pet?.BirthDate;
        WeightBox.Text = pet?.WeightKg?.ToString("0.##", CultureInfo.CurrentCulture) ?? "";
        DeleteButton.IsEnabled = pet is not null;
    }

    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Grid.SelectedItem is Pet pet)
            ShowPet(pet);
    }

    private void New_Click(object sender, RoutedEventArgs e)
    {
        Grid.SelectedItem = null;
        ShowPet(null);
        OwnerBox.Focus();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (OwnerBox.SelectedValue is not int ownerId)
        {
            App.ShowValidation("Choose the pet's owner.");
            return;
        }
        if (string.IsNullOrWhiteSpace(NameBox.Text) || string.IsNullOrWhiteSpace(SpeciesBox.Text))
        {
            App.ShowValidation("Name and species are required.");
            return;
        }
        if (SpeciesBox.Text.Trim().Length > 30)
        {
            App.ShowValidation("Species must be 30 characters or fewer.");
            return;
        }
        if (BirthDateBox.SelectedDate > DateTime.Today)
        {
            App.ShowValidation("Birth date cannot be in the future.");
            return;
        }

        decimal? weight = null;
        if (!string.IsNullOrWhiteSpace(WeightBox.Text))
        {
            if (!decimal.TryParse(WeightBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var w) || w <= 0 || w >= 10000)
            {
                App.ShowValidation("Weight must be a positive number of kilograms (e.g. 12.5).");
                return;
            }
            weight = Math.Round(w, 2);
        }

        var pet = new Pet
        {
            PetId = _current?.PetId ?? 0,
            OwnerId = ownerId,
            Name = NameBox.Text.Trim(),
            Species = SpeciesBox.Text.Trim(),
            Breed = Blank(BreedBox.Text),
            BirthDate = BirthDateBox.SelectedDate,
            WeightKg = weight,
        };

        var isNew = _current is null;
        if (!TryData(() => { if (isNew) App.Pets.Insert(pet); else App.Pets.Update(pet); }))
            return;

        App.Status(isNew ? $"Added pet #{pet.PetId} {pet.Name}." : $"Updated pet #{pet.PetId} {pet.Name}.");
        LoadPets(pet.PetId);
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (_current is not { } pet)
            return;
        if (!App.ConfirmDelete($"Delete pet #{pet.PetId} {pet.Name} (owner: {pet.OwnerName})?\n\nThis also deletes the pet's appointments."))
            return;
        if (!TryData(() => App.Pets.Delete(pet.PetId)))
            return;

        App.Status($"Deleted pet #{pet.PetId} {pet.Name}.");
        LoadPets();
    }
}
