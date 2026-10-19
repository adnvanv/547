using System.Diagnostics;
using HotelReservations.Data;
using HotelReservations.Web.Infrastructure;
using HotelReservations.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservations.Web.Controllers;

public class HomeController(GuestRepository guests, RoomRepository rooms, ReservationRepository reservations,
                            RoomTypeRepository roomTypes) : AppController
{
    public async Task<IActionResult> Index()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var allRooms = await rooms.GetAllAsync();
        var active = (await reservations.GetAllAsync(includeCancelled: false)).ToList();

        return View(new DashboardViewModel
        {
            Today = today,
            GuestCount = (await guests.GetAllAsync()).Count,
            RoomCount = allRooms.Count,
            RoomTypeCount = (await roomTypes.GetAllAsync(activeOnly: true)).Count,
            ReservationCount = active.Count,
            OccupiedTonight = active.Count(r => r.CheckInDate <= today && today < r.CheckOutDate),
            AvailableRooms = allRooms.Count(r => !r.IsOutOfService),
            Arrivals = active.Where(r => r.CheckInDate >= today && r.Status == "Booked")
                             .OrderBy(r => r.CheckInDate).Take(6).ToList(),
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
