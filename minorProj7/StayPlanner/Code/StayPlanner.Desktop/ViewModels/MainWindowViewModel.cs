using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StayPlanner.Business;
using StayPlanner.Data;

namespace StayPlanner.Desktop.ViewModels;

public sealed class RoomItem(RoomAvailability availability)
{
    public Room Room { get; } = availability.Room;
    public bool IsAvailable { get; } = availability.IsAvailable;
    public string Title => $"Room {Room.Number}  ·  {Room.Type}";
    public string Details => $"Floor {Room.Floor}  ·  {Room.Beds} bed(s)  ·  up to {Room.MaxGuests} guests";
    public string Rate => $"{Money.Format(Room.NightlyRate)}/night";
    public string Status => IsAvailable ? "Available" : "Booked";
    public string StatusColor => IsAvailable ? "#1E7F4F" : "#B42318";
}

public sealed class QuoteLineItem(QuoteLine line)
{
    public string Description { get; } = line.Description;
    public string Amount { get; } = Money.Format(line.Amount);
}

public sealed class ReservationItem(Reservation r, Room? room)
{
    public int Id { get; } = r.Id;
    public string Title => $"#{r.Id}  {r.GuestName}  ·  Room {room?.Number ?? r.RoomId.ToString()}";
    public string Details => $"{r.CheckIn:ddd MMM d} → {r.CheckOut:ddd MMM d, yyyy}  ·  {r.Guests} guest(s)  ·  booked on {r.CreatedOn}";
    public string Total => Money.Format(r.Total);
}

public partial class MainWindowViewModel : ObservableObject
{
    private readonly QuoteManager _quotes;
    private readonly ReservationManager _manager;
    private readonly Dictionary<int, Room> _roomsById;

    public MainWindowViewModel(QuoteManager quotes, ReservationManager manager)
    {
        _quotes = quotes;
        _manager = manager;
        _roomsById = manager.GetAllRooms().ToDictionary(r => r.Id);

        var nextFriday = DateTime.Today.AddDays(((int)DayOfWeek.Friday - (int)DateTime.Today.DayOfWeek + 7) % 7);
        if (nextFriday == DateTime.Today) nextFriday = nextFriday.AddDays(7);
        _checkIn = nextFriday;
        _checkOut = nextFriday.AddDays(3);

        RefreshRooms();
        RefreshReservations();
    }

    public string PlatformDescription => PlatformInfo.Description;
    public string StoragePath => _manager.StoragePath;
    public DateTime Today => DateTime.Today;

    public ObservableCollection<RoomItem> Rooms { get; } = [];
    public ObservableCollection<QuoteLineItem> QuoteLines { get; } = [];
    public ObservableCollection<ReservationItem> Reservations { get; } = [];

    [ObservableProperty] private DateTime? _checkIn;
    [ObservableProperty] private DateTime? _checkOut;
    [ObservableProperty] private decimal? _guests = 2;
    [ObservableProperty] private string _guestName = "";
    [ObservableProperty] private RoomItem? _selectedRoom;
    [ObservableProperty] private string _quoteTotal = "";
    [ObservableProperty] private string _quoteHeader = "Select a room to see a price quote.";
    [ObservableProperty] private string _message = "";
    [ObservableProperty] private bool _messageIsError;

    public string MessageColor => MessageIsError ? "#B42318" : "#1E7F4F";

    partial void OnMessageIsErrorChanged(bool value) => OnPropertyChanged(nameof(MessageColor));
    partial void OnCheckInChanged(DateTime? value) => RefreshRooms();
    partial void OnCheckOutChanged(DateTime? value) => RefreshRooms();
    partial void OnGuestsChanged(decimal? value) => RefreshRooms();
    partial void OnSelectedRoomChanged(RoomItem? value) => RefreshQuote();

    private bool TryGetStay(out DateOnly checkIn, out DateOnly checkOut, out int guests)
    {
        checkIn = CheckIn is { } i ? DateOnly.FromDateTime(i) : default;
        checkOut = CheckOut is { } o ? DateOnly.FromDateTime(o) : default;
        guests = (int)(Guests ?? 0);
        return CheckIn is not null && CheckOut is not null && guests > 0;
    }

    private void RefreshRooms()
    {
        var selectedId = SelectedRoom?.Room.Id;
        Rooms.Clear();
        if (TryGetStay(out var checkIn, out var checkOut, out var guests) && checkOut > checkIn)
        {
            var available = _manager.GetRooms(checkIn, checkOut, guests);
            foreach (var a in available) Rooms.Add(new RoomItem(a));
        }
        SelectedRoom = Rooms.FirstOrDefault(r => r.Room.Id == selectedId) ?? Rooms.FirstOrDefault(r => r.IsAvailable);
        RefreshQuote();
    }

    private void RefreshQuote()
    {
        QuoteLines.Clear();
        QuoteTotal = "";
        if (SelectedRoom is null || !TryGetStay(out var checkIn, out var checkOut, out var guests))
        {
            QuoteHeader = "Select a room to see a price quote.";
            return;
        }

        try
        {
            var quote = _quotes.Quote(SelectedRoom.Room.Id, checkIn, checkOut, guests);
            QuoteHeader = $"Room {quote.Room.Number} · {quote.Room.Type} · {quote.Nights} night(s)";
            foreach (var line in quote.Lines) QuoteLines.Add(new QuoteLineItem(line));
            QuoteTotal = Money.Format(quote.Total);
        }
        catch (BusinessRuleException ex)
        {
            QuoteHeader = ex.Message;
        }
    }

    private void RefreshReservations()
    {
        Reservations.Clear();
        foreach (var r in _manager.GetAll())
            Reservations.Add(new ReservationItem(r, _roomsById.GetValueOrDefault(r.RoomId)));
    }

    [RelayCommand]
    private void Book()
    {
        if (SelectedRoom is null || !TryGetStay(out var checkIn, out var checkOut, out var guests))
        {
            Show("Choose dates, guests and a room first.", error: true);
            return;
        }

        try
        {
            var saved = _manager.Book(SelectedRoom.Room.Id, GuestName, checkIn, checkOut, guests);
            Show($"Booked reservation #{saved.Id} for {saved.GuestName}: {Money.Format(saved.Total)}.", error: false);
            GuestName = "";
            RefreshRooms();
            RefreshReservations();
        }
        catch (BusinessRuleException ex)
        {
            Show(ex.Message, error: true);
        }
    }

    [RelayCommand]
    private void Cancel(int reservationId)
    {
        try
        {
            _manager.Cancel(reservationId);
            Show($"Cancelled reservation #{reservationId}.", error: false);
            RefreshRooms();
            RefreshReservations();
        }
        catch (BusinessRuleException ex)
        {
            Show(ex.Message, error: true);
        }
    }

    private void Show(string text, bool error)
    {
        MessageIsError = error;
        Message = text;
    }
}
