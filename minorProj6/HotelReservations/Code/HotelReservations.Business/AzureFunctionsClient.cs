using System.Net.Http.Json;

namespace HotelReservations.Business;

// Calls the HotelReservations.Functions app in Azure. The service layer registers this with
// AddHttpClient, which hands it an HttpClient whose BaseAddress is the function app's URL
// (AzureFunctions:BaseUrl in the API's appsettings.json).

public class AzureFunctionsClient(HttpClient http)
{
    /// <summary>POST {function app}/api/quote</summary>
    public Task<StayQuote> GetStayQuoteAsync(decimal nightlyRate, DateOnly checkIn, DateOnly checkOut, int numberOfGuests) =>
        PostAsync<StayQuote>("api/quote", new { nightlyRate, checkIn, checkOut, numberOfGuests });

    /// <summary>POST {function app}/api/loyalty</summary>
    public Task<LoyaltyStatus> GetLoyaltyStatusAsync(int stays, int nights, decimal totalSpent, DateOnly guestSince) =>
        PostAsync<LoyaltyStatus>("api/loyalty", new { stays, nights, totalSpent, guestSince });

    private async Task<T> PostAsync<T>(string path, object body)
    {
        HttpResponseMessage response;
        try
        {
            response = await http.PostAsJsonAsync(path, body);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ExternalServiceException($"Couldn't reach the Azure function at {http.BaseAddress}{path}: {ex.Message}", ex);
        }

        // The functions answer 400 with { "error": "..." } when the input breaks one of their rules.
        if (!response.IsSuccessStatusCode)
        {
            var error = await ReadErrorAsync(response);
            if ((int)response.StatusCode == 400)
                throw new BusinessRuleException(error ?? "The Azure function rejected the request.");
            throw new ExternalServiceException(
                $"The Azure function at {path} returned {(int)response.StatusCode} {response.ReasonPhrase}. {error}".Trim());
        }

        return await response.Content.ReadFromJsonAsync<T>()
               ?? throw new ExternalServiceException($"The Azure function at {path} returned an empty response.");
    }

    private static async Task<string?> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            return (await response.Content.ReadFromJsonAsync<FunctionError>())?.Error;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private record FunctionError(string? Error);
}
