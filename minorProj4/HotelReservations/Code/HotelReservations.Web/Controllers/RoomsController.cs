using HotelReservations.Data;
using HotelReservations.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelReservations.Web.Controllers;

public class RoomsController(RoomRepository rooms, RoomTypeRepository roomTypes) : AppController
{
    public async Task<IActionResult> Index() => View(await rooms.GetAllAsync());

    public async Task<IActionResult> Edit(int? id)
    {
        var room = id is null ? new Room() : await rooms.GetAsync(id.Value);
        return room is null ? NotFound() : await FormView(room);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Edit(Room room) => SaveAndRedirectAsync(
        async () =>
        {
            if (room.RoomId == 0) room.RoomId = await rooms.AddAsync(room);
            else await rooms.UpdateAsync(room);
        },
        $"{T("Room saved:")} {room.RoomNumber}",
        () => FormView(room));

    public async Task<IActionResult> Delete(int id)
    {
        var room = await rooms.GetAsync(id);
        return room is null ? NotFound() : View(room);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public Task<IActionResult> DeleteConfirmed(int id) =>
        DeleteAndRedirectAsync(() => rooms.DeleteAsync(id), $"{T("Room deleted:")} #{id}");

    /// <summary>Shows the form with the active room types (plus the room's current type, even if inactive).</summary>
    private async Task<IActionResult> FormView(Room room)
    {
        var types = (await roomTypes.GetAllAsync()).Where(t => t.IsActive || t.RoomTypeId == room.RoomTypeId);
        ViewBag.RoomTypes = new SelectList(
            types.Select(t => new { t.RoomTypeId, Text = $"{t.Name} ({t.NightlyRate:C}/night, sleeps {t.MaxOccupancy})" }),
            "RoomTypeId", "Text", room.RoomTypeId);
        return View("Edit", room);
    }
}
