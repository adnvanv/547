using HotelReservations.Data;

namespace HotelReservations.Business;

/// <summary>A request broke a business rule (bad dates, double booking, record in use...). Maps to HTTP 400/409.</summary>
public class BusinessRuleException(string message, Exception? inner = null) : Exception(message, inner);

/// <summary>The record asked for does not exist. Maps to HTTP 404.</summary>
public class NotFoundException(string what, int id) : Exception($"{what} {id} was not found.");

/// <summary>Reservation statuses allowed by CK_Reservations_Status.</summary>
public static class ReservationStatus
{
    public const string Booked = "Booked";
    public const string CheckedIn = "Checked In";
    public const string CheckedOut = "Checked Out";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = [Booked, CheckedIn, CheckedOut, Cancelled];
}

// The business layer hands back these combined records so callers don't have to
// look up a room's type or a reservation's guest themselves.

public record GuestDetails(Guest Guest, int ReservationCount)
{
    public string FullName => $"{Guest.FirstName} {Guest.LastName}";
}

public record RoomTypeDetails(RoomType RoomType, int RoomCount);

public record RoomDetails(Room Room, RoomType RoomType, int ReservationCount);

public record ReservationDetails(Reservation Reservation, Guest Guest, Room Room, RoomType RoomType)
{
    public int Nights => Reservation.CheckOutDate.DayNumber - Reservation.CheckInDate.DayNumber;
    public decimal TotalCost => Nights * RoomType.NightlyRate;
}

/// <summary>A call to an Azure function failed (unreachable, timed out, or a server error). Maps to HTTP 502.</summary>
public class ExternalServiceException(string message, Exception? inner = null) : Exception(message, inner);

// Results from the Azure functions (same JSON shapes as HotelReservations.Functions/Models.cs).

public record NightlyRate(DateOnly Date, string Day, decimal Rate);

public record StayQuote(
    int Nights,
    int WeekendNights,
    decimal RoomSubtotal,
    decimal WeekendSurcharge,
    decimal ExtraGuestFee,
    decimal LengthOfStayDiscount,
    decimal Tax,
    decimal Total,
    List<NightlyRate> NightlyRates);

public record LoyaltyStatus(
    string Tier,
    int Points,
    int DiscountPercent,
    string? NextTier,
    int PointsToNextTier,
    int YearsAsGuest);
