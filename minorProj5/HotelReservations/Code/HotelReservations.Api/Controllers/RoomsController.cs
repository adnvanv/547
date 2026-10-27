using HotelReservations.Api.Models;
using HotelReservations.Business;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservations.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController(RoomManager manager) : ControllerBase
{
    private const string Name = "Room";

    /// <summary>GET api/rooms</summary>
    [HttpGet]
    public async Task<IEnumerable<RoomModel>> GetAll() => (await manager.GetAllAsync()).Select(d => d.ToModel());

    /// <summary>GET api/rooms/5</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomModel>> Get(int id) =>
        (await manager.GetAsync(id))?.ToModel() ?? throw new NotFoundException(Name, id);

    /// <summary>GET api/rooms/available?checkIn=2026-11-01&amp;checkOut=2026-11-03&amp;guests=2</summary>
    [HttpGet("available")]
    public async Task<IEnumerable<RoomModel>> GetAvailable(DateOnly checkIn, DateOnly checkOut, int guests = 1) =>
        (await manager.GetAvailableAsync(checkIn, checkOut, guests)).Select(d => d.ToModel());

    /// <summary>POST api/rooms - inserts when id is 0, otherwise updates.</summary>
    [HttpPost]
    public async Task<ActionResult<SaveResult<RoomModel>>> Save(RoomModel model)
    {
        var isNew = model.Id == 0;
        var id = await manager.SaveAsync(model.ToEntity());
        var saved = new SaveResult<RoomModel>(isNew ? "created" : "updated", (await manager.GetAsync(id))!.ToModel());
        return isNew ? CreatedAtAction(nameof(Get), new { id }, saved) : Ok(saved);
    }

    /// <summary>DELETE api/rooms/5</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await manager.DeleteAsync(id);
        return NoContent();
    }
}
