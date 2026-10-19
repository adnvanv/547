using HotelReservations.Data;

namespace HotelReservations.Web.Models;

public class GuestSearchViewModel
{
    public string? Search { get; set; }
    public GuestSearchField Field { get; set; }
    public MatchMode Match { get; set; }
    public List<Guest> Results { get; set; } = [];

    public bool IsSearching => !string.IsNullOrWhiteSpace(Search);
}
