using BookSpace.Core.Services;
using BookSpace.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookSpace.Web.Controllers.Api;

/// <summary>ოთახები და მათი თავისუფალი დროები.</summary>
[Route("api/v1/rooms")]
public class RoomsApiController : ApiControllerBase
{
    private readonly RoomService _rooms;
    private readonly AvailabilityService _availability;

    public RoomsApiController(RoomService rooms, AvailabilityService availability)
    {
        _rooms = rooms;
        _availability = availability;
    }

    /// <summary>აქტიური ოთახების სია (US-03). ავტორიზაცია არ სჭირდება.</summary>
    /// <response code="200">ოთახების სია (შეიძლება ცარიელი იყოს).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoomDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms(CancellationToken ct) =>
        Ok((await _rooms.GetActiveRoomsAsync(ct)).Select(RoomDto.From));

    /// <summary>ოთახის საათობრივი სლოტები არჩეულ დღეს (US-04).</summary>
    /// <param name="id">ოთახის id.</param>
    /// <param name="date">თარიღი ფორმატით yyyy-MM-dd, მაგ. 2026-11-11.</param>
    /// <param name="ct">გაუქმების ტოკენი.</param>
    /// <response code="200">სლოტები: Free / Booked / Past.</response>
    /// <response code="404">ოთახი ვერ მოიძებნა ან დახურულია.</response>
    [HttpGet("{id:int}/availability")]
    [ProducesResponseType(typeof(AvailabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvailabilityDto>> GetAvailability(int id, [FromQuery] DateOnly date,
        CancellationToken ct)
    {
        var result = await _availability.GetScheduleAsync(id, date, ct);
        return result.IsSuccess ? Ok(AvailabilityDto.From(result.Value!)) : ErrorResult(result);
    }
}
