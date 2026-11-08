namespace HotelReservations.Functions;

// Request and response shapes for the functions. The business layer sends and reads the same JSON
// (camelCase), so these are the contract between HotelReservations.Business and Azure.

public record QuoteRequest(decimal NightlyRate, DateOnly CheckIn, DateOnly CheckOut, int NumberOfGuests);

public record QuoteNight(DateOnly Date, string Day, decimal Rate);

public record QuoteResponse(
    int Nights,
    int WeekendNights,
    decimal RoomSubtotal,
    decimal WeekendSurcharge,
    decimal ExtraGuestFee,
    decimal LengthOfStayDiscount,
    decimal Tax,
    decimal Total,
    IReadOnlyList<QuoteNight> NightlyRates);

public record LoyaltyRequest(int Stays, int Nights, decimal TotalSpent, DateOnly GuestSince);

public record LoyaltyResponse(
    string Tier,
    int Points,
    int DiscountPercent,
    string? NextTier,
    int PointsToNextTier,
    int YearsAsGuest);
