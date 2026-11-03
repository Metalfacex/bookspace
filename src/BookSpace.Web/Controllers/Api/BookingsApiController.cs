using BookSpace.Core.Services;
using BookSpace.Web.Auth;
using BookSpace.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookSpace.Web.Controllers.Api;

[Route("api/bookings")]
[Authorize]
public class BookingsApiController : ApiControllerBase
{
    private readonly BookingService _bookings;

    public BookingsApiController(BookingService bookings)
    {
        _bookings = bookings;
    }

    /// <summary>POST /api/bookings — ჯავშნის შექმნა (US-05). 201 / 400 / 404 / 409 / 422.</summary>
    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        var result = await _bookings.BookAsync(User.GetUserId(), request.RoomId, request.Date!.Value,
            request.StartHour, ct);
        if (!result.IsSuccess)
        {
            return ErrorResult(result);
        }

        var dto = BookingDto.From(result.Value!);
        return Created($"/api/bookings/{dto.Id}", dto);
    }

    /// <summary>GET /api/bookings/me — ჩემი ჯავშნები (US-06).</summary>
    [HttpGet("me")]
    public async Task<ActionResult<MyBookingsDto>> Mine(CancellationToken ct)
    {
        var my = await _bookings.GetMyBookingsAsync(User.GetUserId(), ct);
        return Ok(new MyBookingsDto(
            my.Upcoming.Select(BookingDto.From).ToList(),
            my.History.Select(BookingDto.From).ToList()));
    }

    /// <summary>DELETE /api/bookings/{id} — გაუქმება (US-07). 204 / 403 / 404 / 409.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var result = await _bookings.CancelAsync(User.GetUserId(), id, ct);
        return result.IsSuccess ? NoContent() : ErrorResult(result);
    }
}
