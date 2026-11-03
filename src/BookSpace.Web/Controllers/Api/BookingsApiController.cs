using BookSpace.Core.Services;
using BookSpace.Web.Auth;
using BookSpace.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookSpace.Web.Controllers.Api;

/// <summary>ავტორიზებული სტუდენტის ჯავშნები.</summary>
[Route("api/v1/bookings")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class BookingsApiController : ApiControllerBase
{
    private readonly BookingService _bookings;

    public BookingsApiController(BookingService bookings)
    {
        _bookings = bookings;
    }

    /// <summary>ჯავშნის შექმნა (US-05).</summary>
    /// <response code="201">ჯავშანი შეიქმნა; Location = /api/v1/bookings/{id}.</response>
    /// <response code="400">არავალიდური საათი ან თარიღი (წარსული, სამუშაო საათებს გარეთ, 14 დღეზე შორს).</response>
    /// <response code="404">ოთახი ვერ მოიძებნა.</response>
    /// <response code="409">სლოტი უკვე დაკავებულია.</response>
    /// <response code="422">დღიური ლიმიტი (3 ჯავშანი) ამოწურულია.</response>
    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BookingDto>> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        var result = await _bookings.BookAsync(User.GetUserId(), request.RoomId, request.Date!.Value,
            request.StartHour, ct);
        if (!result.IsSuccess)
        {
            return ErrorResult(result);
        }

        var dto = BookingDto.From(result.Value!);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    /// <summary>ჩემი ერთი ჯავშანი id-ით.</summary>
    /// <response code="200">ჯავშანი.</response>
    /// <response code="404">ასეთი ჯავშანი შენ არ გაქვს.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDto>> GetById(int id, CancellationToken ct)
    {
        var my = await _bookings.GetMyBookingsAsync(User.GetUserId(), ct);
        var booking = my.Upcoming.Concat(my.History).FirstOrDefault(b => b.Id == id);
        return booking is null
            ? NotFound(new ErrorDto("NotFound", "ჯავშანი ვერ მოიძებნა."))
            : Ok(BookingDto.From(booking));
    }

    /// <summary>ჩემი მომავალი ჯავშნები და ისტორია (US-06).</summary>
    /// <response code="200">upcoming + history.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(MyBookingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MyBookingsDto>> Mine(CancellationToken ct)
    {
        var my = await _bookings.GetMyBookingsAsync(User.GetUserId(), ct);
        return Ok(new MyBookingsDto(
            my.Upcoming.Select(BookingDto.From).ToList(),
            my.History.Select(BookingDto.From).ToList()));
    }

    /// <summary>ჯავშნის გაუქმება (US-07).</summary>
    /// <response code="204">გაუქმდა.</response>
    /// <response code="400">ჯავშანი უკვე დაიწყო ან წარსულშია.</response>
    /// <response code="403">ჯავშანი სხვას ეკუთვნის.</response>
    /// <response code="404">ჯავშანი ვერ მოიძებნა.</response>
    /// <response code="409">უკვე გაუქმებულია.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var result = await _bookings.CancelAsync(User.GetUserId(), id, ct);
        return result.IsSuccess ? NoContent() : ErrorResult(result);
    }
}
