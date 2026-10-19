using HotelReservations.Data;

namespace HotelReservations.Web.Models;

public class DashboardViewModel
{
    public DateOnly Today { get; set; }
    public int GuestCount { get; set; }
    public int RoomCount { get; set; }
    public int RoomTypeCount { get; set; }
    public int ReservationCount { get; set; }
    public int OccupiedTonight { get; set; }
    public int AvailableRooms { get; set; }
    public List<Reservation> Arrivals { get; set; } = [];
}
