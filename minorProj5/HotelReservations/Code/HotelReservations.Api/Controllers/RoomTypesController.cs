using HotelReservations.Api.Models;
using HotelReservations.Business;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservations.Api.Controllers;

[ApiController]
[Route("api/roomtypes")]
public class RoomTypesController(RoomTypeManager manager) : ControllerBase
{
    private const string Name = "Room type";

    /// <summary>GET api/roomtypes?activeOnly=true</summary>
    [HttpGet]
    public async Task<IEnumerable<RoomTypeModel>> GetAll(bool activeOnly = false) =>
        (await manager.GetAllAsync(activeOnly)).Select(d => d.ToModel());

    /// <summary>GET api/roomtypes/3</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomTypeModel>> Get(int id) =>
        (await manager.GetAsync(id))?.ToModel() ?? throw new NotFoundException(Name, id);

    /// <summary>POST api/roomtypes - inserts when id is 0, otherwise updates.</summary>
    [HttpPost]
    public async Task<ActionResult<SaveResult<RoomTypeModel>>> Save(RoomTypeModel model)
    {
        var isNew = model.Id == 0;
        var id = await manager.SaveAsync(model.ToEntity());
        var saved = new SaveResult<RoomTypeModel>(isNew ? "created" : "updated", (await manager.GetAsync(id))!.ToModel());
        return isNew ? CreatedAtAction(nameof(Get), new { id }, saved) : Ok(saved);
    }

    /// <summary>DELETE api/roomtypes/3</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await manager.DeleteAsync(id);
        return NoContent();
    }
}
