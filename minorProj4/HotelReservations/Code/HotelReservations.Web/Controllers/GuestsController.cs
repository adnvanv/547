using HotelReservations.Data;
using HotelReservations.Web.Infrastructure;
using HotelReservations.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservations.Web.Controllers;

public class GuestsController(GuestRepository guests, ReservationRepository reservations) : AppController
{
    // GET /Guests?search=smith&field=LastName&match=StartsWith
    public async Task<IActionResult> Index(string? search, GuestSearchField field = GuestSearchField.Any,
                                           MatchMode match = MatchMode.Contains) =>
        View(new GuestSearchViewModel
        {
            Search = search,
            Field = field,
            Match = match,
            Results = await guests.SearchAsync(search, field, match),
        });

    public async Task<IActionResult> Details(int id)
    {
        var guest = await guests.GetAsync(id);
        if (guest is null) return NotFound();
        ViewBag.Reservations = await reservations.GetForGuestAsync(id);
        return View(guest);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return View(new Guest());
        var guest = await guests.GetAsync(id.Value);
        return guest is null ? NotFound() : View(guest);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Edit(Guest guest) => SaveAndRedirectAsync(
        async () =>
        {
            if (guest.GuestId == 0) guest.GuestId = await guests.AddAsync(guest);
            else await guests.UpdateAsync(guest);
        },
        $"{T("Guest saved:")} {guest.FullName}",
        () => Task.FromResult<IActionResult>(View(guest)));

    public async Task<IActionResult> Delete(int id)
    {
        var guest = await guests.GetAsync(id);
        return guest is null ? NotFound() : View(guest);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public Task<IActionResult> DeleteConfirmed(int id) =>
        DeleteAndRedirectAsync(() => guests.DeleteAsync(id), $"{T("Guest deleted:")} #{id}");
}
