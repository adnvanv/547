using HotelReservations.Data;
using HotelReservations.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservations.Web.Controllers;

/// <summary>Room types can only be changed here, and only in Administrator mode.</summary>
[AdminOnly]
public class AdminController(RoomTypeRepository roomTypes) : AppController
{
    public async Task<IActionResult> Index() => View(await roomTypes.GetAllAsync());

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return View(new RoomType());
        var type = await roomTypes.GetAsync(id.Value);
        return type is null ? NotFound() : View(type);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Edit(RoomType roomType) => SaveAndRedirectAsync(
        async () =>
        {
            if (roomType.RoomTypeId == 0) roomType.RoomTypeId = await roomTypes.AddAsync(roomType);
            else await roomTypes.UpdateAsync(roomType);
        },
        $"{T("Room type saved:")} {roomType.Name}",
        () => Task.FromResult<IActionResult>(View(roomType)));

    public async Task<IActionResult> Delete(int id)
    {
        var type = await roomTypes.GetAsync(id);
        return type is null ? NotFound() : View(type);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public Task<IActionResult> DeleteConfirmed(int id) =>
        DeleteAndRedirectAsync(() => roomTypes.DeleteAsync(id), $"{T("Room type deleted:")} #{id}");

    [AllowWithoutAdminMode]
    public IActionResult Locked() => View();
}

