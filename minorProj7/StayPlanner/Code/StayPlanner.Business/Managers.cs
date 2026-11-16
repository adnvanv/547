using System.Runtime.InteropServices;
using System.Globalization;
using StayPlanner.Data;

namespace StayPlanner.Business;

/// <summary>Thrown when a request breaks a business rule; the message is safe to show to the user.</summary>
public sealed class BusinessRuleException(string message) : Exception(message);

public sealed record QuoteLine(string Description, decimal Amount);

public sealed record StayQuote(Room Room, DateOnly CheckIn, DateOnly CheckOut, int Nights, IReadOnlyList<QuoteLine> Lines, decimal Total);

/// <summary>Prices a stay: nightly rate, 20% Friday/Saturday surcharge, 7-night discount, extra-guest fee and tax.</summary>
public sealed class QuoteManager(IRoomRepository rooms)
{
    public const decimal WeekendSurcharge = 0.20m;
    public const decimal WeeklyDiscount = 0.10m;
    public const decimal ExtraGuestFee = 15m;
    public const decimal TaxRate = 0.0875m;
    public const int MaxNights = 30;

    public StayQuote Quote(int roomId, DateOnly checkIn, DateOnly checkOut, int guests)
    {
        var room = rooms.SelectById(roomId) ?? throw new BusinessRuleException($"Room {roomId} does not exist.");
        var nights = checkOut.DayNumber - checkIn.DayNumber;

        if (nights <= 0) throw new BusinessRuleException("Check-out must be after check-in.");
        if (nights > MaxNights) throw new BusinessRuleException($"Stays are limited to {MaxNights} nights.");
        if (guests < 1) throw new BusinessRuleException("At least one guest is required.");
        if (guests > room.MaxGuests) throw new BusinessRuleException($"Room {room.Number} holds at most {room.MaxGuests} guests.");

        var weekendNights = Enumerable.Range(0, nights)
            .Select(checkIn.AddDays)
            .Count(d => d.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday);

        var lines = new List<QuoteLine>
        {
            new($"{nights} night(s) x {Money.Format(room.NightlyRate)}", nights * room.NightlyRate),
        };
        if (weekendNights > 0)
            lines.Add(new($"Weekend surcharge ({weekendNights} night(s) x 20%)", Round(weekendNights * room.NightlyRate * WeekendSurcharge)));
        if (nights >= 7)
            lines.Add(new("Weekly stay discount (10%)", -Round(lines.Sum(l => l.Amount) * WeeklyDiscount)));
        var extraGuests = Math.Max(0, guests - 2);
        if (extraGuests > 0)
            lines.Add(new($"Extra guests ({extraGuests} x {Money.Format(ExtraGuestFee)} x {nights} night(s))", extraGuests * ExtraGuestFee * nights));

        var subtotal = lines.Sum(l => l.Amount);
        lines.Add(new($"Tax ({TaxRate * 100:0.##}%)", Round(subtotal * TaxRate)));

        return new StayQuote(room, checkIn, checkOut, nights, lines, lines.Sum(l => l.Amount));
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

public sealed record RoomAvailability(Room Room, bool IsAvailable);

/// <summary>Books and cancels stays, and rejects double bookings.</summary>
public sealed class ReservationManager(IRoomRepository rooms, IReservationRepository reservations, QuoteManager quotes)
{
    public string StoragePath => reservations.StoragePath;

    public IReadOnlyList<Room> GetAllRooms() => rooms.SelectAll();

    public IReadOnlyList<Reservation> GetAll() =>
        reservations.SelectAll().OrderBy(r => r.CheckIn).ThenBy(r => r.RoomId).ToList();

    public IReadOnlyList<RoomAvailability> GetRooms(DateOnly checkIn, DateOnly checkOut, int guests)
    {
        var booked = reservations.SelectAll();
        return rooms.SelectAll()
            .Where(r => r.MaxGuests >= guests)
            .Select(r => new RoomAvailability(r, !booked.Any(b => b.RoomId == r.Id && Overlaps(b, checkIn, checkOut))))
            .ToList();
    }

    public Reservation Book(int roomId, string guestName, DateOnly checkIn, DateOnly checkOut, int guests)
    {
        guestName = guestName.Trim();
        if (guestName.Length == 0) throw new BusinessRuleException("Enter the guest's name.");
        if (guestName.Length > 80) throw new BusinessRuleException("Guest name must be 80 characters or fewer.");
        if (checkIn < DateOnly.FromDateTime(DateTime.Today)) throw new BusinessRuleException("Check-in cannot be in the past.");

        var quote = quotes.Quote(roomId, checkIn, checkOut, guests);

        if (reservations.SelectAll().Any(b => b.RoomId == roomId && Overlaps(b, checkIn, checkOut)))
            throw new BusinessRuleException($"Room {quote.Room.Number} is already booked for some of those nights.");

        return reservations.Insert(new Reservation(0, roomId, guestName, guests, checkIn, checkOut, quote.Total,
            DateTime.UtcNow, PlatformInfo.ShortName));
    }

    public void Cancel(int reservationId)
    {
        if (!reservations.Delete(reservationId))
            throw new BusinessRuleException($"Reservation {reservationId} was not found.");
    }

    private static bool Overlaps(Reservation r, DateOnly checkIn, DateOnly checkOut) =>
        r.CheckIn < checkOut && checkIn < r.CheckOut;
}

/// <summary>Describes the machine the app is running on, so screenshots show which OS produced them.</summary>
public static class PlatformInfo
{
    public static string ShortName =>
        OperatingSystem.IsWindows() ? "Windows" :
        OperatingSystem.IsLinux() ? "Linux" :
        OperatingSystem.IsMacOS() ? "macOS" : RuntimeInformation.OSDescription;

    public static string Description =>
        $"{RuntimeInformation.OSDescription} | {RuntimeInformation.OSArchitecture} | {RuntimeInformation.RuntimeIdentifier} | .NET {Environment.Version}";
}

/// <summary>US-dollar formatting that does not depend on the OS culture settings (a bare Linux image has none).</summary>
public static class Money
{
    public static string Format(decimal value) =>
        (value < 0 ? "-$" : "$") + Math.Abs(value).ToString("N2", CultureInfo.InvariantCulture);
}
