using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace HotelReservations.Functions;

/// <summary>
/// POST api/loyalty - works out a guest's loyalty tier from their stay history. No storage.
/// Points: 1 per dollar spent, 50 per night and 100 per stay. Tiers unlock a standing discount.
/// </summary>
public class LoyaltyFunction(ILogger<LoyaltyFunction> logger)
{
    private static readonly (string Name, int MinPoints, int DiscountPercent)[] Tiers =
    [
        ("Member", 0, 0),
        ("Silver", 1_000, 5),
        ("Gold", 3_000, 10),
        ("Platinum", 7_500, 15),
    ];

    [Function("Loyalty")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "loyalty")] HttpRequest req)
    {
        var history = await req.ReadFromJsonAsync<LoyaltyRequest>();
        if (history is null || history.Stays < 0 || history.Nights < 0 || history.TotalSpent < 0)
            return new BadRequestObjectResult(new { error = "Stays, nights and total spent can't be negative." });

        var points = (int)Math.Floor(history.TotalSpent) + history.Nights * 50 + history.Stays * 100;
        var tierIndex = Array.FindLastIndex(Tiers, t => points >= t.MinPoints);
        var tier = Tiers[tierIndex];
        var next = tierIndex + 1 < Tiers.Length ? Tiers[tierIndex + 1] : default;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var years = today.Year - history.GuestSince.Year - (today < history.GuestSince.AddYears(today.Year - history.GuestSince.Year) ? 1 : 0);

        logger.LogInformation("{Points} points -> {Tier}", points, tier.Name);
        return new OkObjectResult(new LoyaltyResponse(tier.Name, points, tier.DiscountPercent,
            next.Name, next.Name is null ? 0 : next.MinPoints - points, Math.Max(0, years)));
    }
}
