using HotelReservations.Data;
using HotelReservations.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelReservations.Web.Controllers;

public class ReservationsController(ReservationRepository reservations, GuestRepository guests,
                                    RoomRepository rooms) : AppController
{
    public async Task<IActionResult> Index() =>
        View(await reservations.GetAllAsync(includeCancelled: Settings.ShowCancelled));

    // "guestId" pre-selects the guest when booking from a guest's details page.
    public async Task<IActionResult> Edit(int? id, int? guestId)
    {
        var reservation = id is null
            ? new Reservation { GuestId = guestId ?? 0 }
            : await reservations.GetAsync(id.Value);
        return reservation is null ? NotFound() : await FormView(reservation);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Edit(Reservation reservation) => SaveAndRedirectAsync(
        async () =>
        {
            if (reservation.ReservationId == 0) reservation.ReservationId = await reservations.AddAsync(reservation);
            else await reservations.UpdateAsync(reservation);
        },
        T("Reservation saved."),
        () => FormView(reservation));

    public async Task<IActionResult> Delete(int id)
    {
        var reservation = await reservations.GetAsync(id);
        return reservation is null ? NotFound() : View(reservation);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public Task<IActionResult> DeleteConfirmed(int id) =>
        DeleteAndRedirectAsync(() => reservations.DeleteAsync(id), $"{T("Reservation deleted:")} #{id}");

    private async Task<IActionResult> FormView(Reservation reservation)
    {
        ViewBag.Guests = new SelectList(
            (await guests.GetAllAsync()).Select(g => new { g.GuestId, Text = $"{g.LastName}, {g.FirstName} ({g.Email})" }),
            "GuestId", "Text", reservation.GuestId);
        ViewBag.Rooms = new SelectList(
            (await rooms.GetAllAsync())
                .Where(r => !r.IsOutOfService || r.RoomId == reservation.RoomId)
                .Select(r => new { r.RoomId, Text = $"{r.RoomNumber} - {r.RoomTypeName} ({r.NightlyRate:C}/night, sleeps {r.MaxOccupancy})" }),
            "RoomId", "Text", reservation.RoomId);
        ViewBag.Statuses = new SelectList(Reservation.Statuses, reservation.Status);
        return View("Edit", reservation);
    }
}
