using HotelReservations.Api.Models;
using HotelReservations.Business;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservations.Api.Controllers;

[ApiController]
[Route("api/guests")]
public class GuestsController(GuestManager manager, ReservationManager reservations) : ControllerBase
{
    private const string Name = "Guest";

    /// <summary>GET api/guests, or GET api/guests?search=smith to filter by name, email, phone or city.</summary>
    [HttpGet]
    public async Task<IEnumerable<GuestModel>> GetAll(string? search = null) =>
        (await manager.SearchAsync(search)).Select(d => d.ToModel());

    /// <summary>GET api/guests/7</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GuestModel>> Get(int id) =>
        (await manager.GetAsync(id))?.ToModel() ?? throw new NotFoundException(Name, id);

    /// <summary>GET api/guests/7/reservations</summary>
    [HttpGet("{id:int}/reservations")]
    public async Task<ActionResult<IEnumerable<ReservationModel>>> GetReservations(int id)
    {
        _ = await manager.GetAsync(id) ?? throw new NotFoundException(Name, id);
        return Ok((await reservations.GetForGuestAsync(id)).Select(d => d.ToModel()));
    }

    /// <summary>POST api/guests - inserts when id is 0, otherwise updates.</summary>
    [HttpPost]
    public async Task<ActionResult<SaveResult<GuestModel>>> Save(GuestModel model)
    {
        var isNew = model.Id == 0;
        var id = await manager.SaveAsync(model.ToEntity());
        var saved = new SaveResult<GuestModel>(isNew ? "created" : "updated", (await manager.GetAsync(id))!.ToModel());
        return isNew ? CreatedAtAction(nameof(Get), new { id }, saved) : Ok(saved);
    }

    /// <summary>DELETE api/guests/7</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await manager.DeleteAsync(id);
        return NoContent();
    }
}
