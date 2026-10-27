using HotelReservations.Api.Models;
using HotelReservations.Business;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservations.Api.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationsController(ReservationManager manager) : ControllerBase
{
    private const string Name = "Reservation";

    /// <summary>GET api/reservations?includeCancelled=false</summary>
    [HttpGet]
    public async Task<IEnumerable<ReservationModel>> GetAll(bool includeCancelled = true) =>
        (await manager.GetAllAsync(includeCancelled)).Select(d => d.ToModel());

    /// <summary>GET api/reservations/12</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationModel>> Get(int id) =>
        (await manager.GetAsync(id))?.ToModel() ?? throw new NotFoundException(Name, id);

    /// <summary>POST api/reservations - inserts when id is 0, otherwise updates.</summary>
    [HttpPost]
    public async Task<ActionResult<SaveResult<ReservationModel>>> Save(ReservationModel model)
    {
        var isNew = model.Id == 0;
        var id = await manager.SaveAsync(model.ToEntity());
        var saved = new SaveResult<ReservationModel>(isNew ? "created" : "updated", (await manager.GetAsync(id))!.ToModel());
        return isNew ? CreatedAtAction(nameof(Get), new { id }, saved) : Ok(saved);
    }

    /// <summary>POST api/reservations/12/cancel - keeps the record but frees the room.</summary>
    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<ReservationModel>> Cancel(int id)
    {
        await manager.CancelAsync(id);
        return (await manager.GetAsync(id))!.ToModel();
    }

    /// <summary>DELETE api/reservations/12</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await manager.DeleteAsync(id);
        return NoContent();
    }
}
