using BookSpace.Core.Services;
using BookSpace.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookSpace.Web.Controllers.Api;

[Route("api/rooms")]
public class RoomsApiController : ApiControllerBase
{
    private readonly RoomService _rooms;
    private readonly AvailabilityService _availability;

    public RoomsApiController(RoomService rooms, AvailabilityService availability)
    {
        _rooms = rooms;
        _availability = availability;
    }

    /// <summary>GET /api/rooms — აქტიური ოთახების სია (US-03).</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms(CancellationToken ct) =>
        Ok((await _rooms.GetActiveRoomsAsync(ct)).Select(RoomDto.From));

    /// <summary>GET /api/rooms/{id}/availability?date=2026-11-10 — სლოტები (US-04).</summary>
    [HttpGet("{id:int}/availability")]
    public async Task<ActionResult<AvailabilityDto>> GetAvailability(int id, [FromQuery] DateOnly date,
        CancellationToken ct)
    {
        var result = await _availability.GetScheduleAsync(id, date, ct);
        return result.IsSuccess ? Ok(AvailabilityDto.From(result.Value!)) : ErrorResult(result);
    }
}
