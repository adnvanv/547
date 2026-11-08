using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace HotelReservations.Functions;

/// <summary>
/// POST api/quote - prices a stay night by night. No storage: everything it needs is in the request.
/// Fridays and Saturdays cost 15% more, each guest past two adds $20 a night, stays of 7+ nights
/// get 10% off, and 12% occupancy tax is added at the end.
/// </summary>
public class StayQuoteFunction(ILogger<StayQuoteFunction> logger)
{
    public const decimal WeekendSurchargeRate = 0.15m;
    public const decimal ExtraGuestFeePerNight = 20m;
    public const int IncludedGuests = 2;
    public const int LongStayNights = 7;
    public const decimal LongStayDiscountRate = 0.10m;
    public const decimal TaxRate = 0.12m;
    public const int MaxNights = 60;

    [Function("StayQuote")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "quote")] HttpRequest req)
    {
        var quote = await req.ReadFromJsonAsync<QuoteRequest>();
        var nights = quote is null ? 0 : quote.CheckOut.DayNumber - quote.CheckIn.DayNumber;
        if (quote is null || nights < 1)
            return new BadRequestObjectResult(new { error = "Check-out must be after check-in." });
        if (nights > MaxNights)
            return new BadRequestObjectResult(new { error = $"Quotes are limited to {MaxNights} nights." });
        if (quote.NightlyRate < 0 || quote.NumberOfGuests < 1)
            return new BadRequestObjectResult(new { error = "Nightly rate can't be negative and a stay needs at least one guest." });

        var nightly = Enumerable.Range(0, nights)
            .Select(i => quote.CheckIn.AddDays(i))
            .Select(date => new QuoteNight(date, date.DayOfWeek.ToString(),
                IsWeekend(date) ? Round(quote.NightlyRate * (1 + WeekendSurchargeRate)) : quote.NightlyRate))
            .ToList();

        var weekendNights = nightly.Count(n => IsWeekend(n.Date));
        var roomSubtotal = quote.NightlyRate * nights;
        var weekendSurcharge = nightly.Sum(n => n.Rate) - roomSubtotal;
        var extraGuestFee = Math.Max(0, quote.NumberOfGuests - IncludedGuests) * ExtraGuestFeePerNight * nights;
        var beforeDiscount = roomSubtotal + weekendSurcharge + extraGuestFee;
        var discount = nights >= LongStayNights ? Round(beforeDiscount * LongStayDiscountRate) : 0;
        var tax = Round((beforeDiscount - discount) * TaxRate);

        logger.LogInformation("Quoted {Nights} night(s) from {CheckIn} at {Rate}", nights, quote.CheckIn, quote.NightlyRate);
        return new OkObjectResult(new QuoteResponse(nights, weekendNights, roomSubtotal, weekendSurcharge,
            extraGuestFee, discount, tax, beforeDiscount - discount + tax, nightly));
    }

    private static bool IsWeekend(DateOnly date) => date.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday;

    private static decimal Round(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
