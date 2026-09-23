using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using VetClinic.Data;
using static VetClinic.App.Views.ViewHelpers;

namespace VetClinic.App.Views;

public partial class AppointmentsView : UserControl
{
    private static readonly string[] TimeFormats = ["h:mm tt", "h:mmtt", "h tt", "htt", "H:mm", "HH:mm"];

    private Appointment? _current;
    private List<Veterinarian> _allVets = [];

    public AppointmentsView()
    {
        InitializeComponent();
        StatusBox.ItemsSource = AppointmentStatus.All;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => LoadAppointments(_current?.AppointmentId);

    private void LoadAppointments(int? selectId = null)
    {
        List<Appointment> appointments = [];
        List<Pet> pets = [];
        if (!TryData(() =>
            {
                appointments = App.Appointments.GetAll();
                pets = App.Pets.GetAll();
                _allVets = App.Veterinarians.GetAll();
            }))
            return;

        PetBox.ItemsSource = pets;
        Grid.ItemsSource = appointments;
        ResultCount.Text = $"{appointments.Count} appointment(s)";

        var match = appointments.FirstOrDefault(a => a.AppointmentId == selectId);
        if (match is not null)
        {
            Grid.SelectedItem = match;
            ShowAppointment(match); // selection may not change if it is the same row
        }
        else
        {
            ShowAppointment(null);
        }
    }

    private void ShowAppointment(Appointment? appointment)
    {
        _current = appointment;

        // Active vets can be booked; an inactive vet stays listed only on appointments already assigned to them.
        VetBox.ItemsSource = _allVets.Where(v => v.IsActive || v.VeterinarianId == appointment?.VeterinarianId).ToList();

        FormTitle.Text = appointment is null ? "New appointment" : $"Edit appointment #{appointment.AppointmentId}";
        IdBox.Text = appointment?.AppointmentId.ToString() ?? "(assigned on save)";
        PetBox.SelectedValue = appointment?.PetId;
        VetBox.SelectedValue = appointment?.VeterinarianId;
        DateBox.SelectedDate = appointment?.ScheduledAt.Date ?? DateTime.Today.AddDays(1);
        TimeBox.Text = (appointment?.ScheduledAt ?? DateTime.Today.AddHours(9)).ToString("h:mm tt", CultureInfo.CurrentCulture);
        ReasonBox.Text = appointment?.Reason ?? "";
        StatusBox.SelectedItem = appointment?.Status ?? AppointmentStatus.Scheduled;
        CostBox.Text = appointment?.Cost?.ToString("0.00", CultureInfo.CurrentCulture) ?? "";
        DeleteButton.IsEnabled = appointment is not null;
    }

    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Grid.SelectedItem is Appointment appointment)
            ShowAppointment(appointment);
    }

    private void New_Click(object sender, RoutedEventArgs e)
    {
        Grid.SelectedItem = null;
        ShowAppointment(null);
        PetBox.Focus();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (PetBox.SelectedValue is not int petId || VetBox.SelectedValue is not int vetId)
        {
            App.ShowValidation("Choose a pet and a veterinarian.");
            return;
        }
        if (DateBox.SelectedDate is not { } date)
        {
            App.ShowValidation("Choose the appointment date.");
            return;
        }
        if (!DateTime.TryParseExact(TimeBox.Text.Trim(), TimeFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces, out var time))
        {
            App.ShowValidation("Enter the time like 9:30 AM or 14:00.");
            return;
        }
        if (string.IsNullOrWhiteSpace(ReasonBox.Text))
        {
            App.ShowValidation("Enter the reason for the visit.");
            return;
        }

        decimal? cost = null;
        if (!string.IsNullOrWhiteSpace(CostBox.Text))
        {
            if (!decimal.TryParse(CostBox.Text.TrimStart('$'), NumberStyles.Number, CultureInfo.CurrentCulture, out var c)
                || c < 0 || c >= 1_000_000)
            {
                App.ShowValidation("Cost must be a dollar amount of zero or more (e.g. 85.00).");
                return;
            }
            cost = Math.Round(c, 2);
        }

        var appointment = new Appointment
        {
            AppointmentId = _current?.AppointmentId ?? 0,
            PetId = petId,
            VeterinarianId = vetId,
            ScheduledAt = date.Date + time.TimeOfDay,
            Reason = ReasonBox.Text.Trim(),
            Status = StatusBox.SelectedItem as string ?? AppointmentStatus.Scheduled,
            Cost = cost,
        };

        var isNew = _current is null;
        if (!TryData(() => { if (isNew) App.Appointments.Insert(appointment); else App.Appointments.Update(appointment); }))
            return;

        App.Status(isNew ? $"Added appointment #{appointment.AppointmentId}." : $"Updated appointment #{appointment.AppointmentId}.");
        LoadAppointments(appointment.AppointmentId);
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (_current is not { } appointment)
            return;
        if (!App.ConfirmDelete($"Delete appointment #{appointment.AppointmentId} for {appointment.PetName} on {appointment.ScheduledAt:g}?"))
            return;
        if (!TryData(() => App.Appointments.Delete(appointment.AppointmentId)))
            return;

        App.Status($"Deleted appointment #{appointment.AppointmentId}.");
        LoadAppointments();
    }
}
