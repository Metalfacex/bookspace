using BookSpace.Core.Services;
using BookSpace.Web.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookSpace.Web.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly BookingService _bookings;

    public BookingsController(BookingService bookings)
    {
        _bookings = bookings;
    }

    // US-05
    [HttpPost]
    public async Task<IActionResult> Create(int roomId, DateOnly date, int startHour, CancellationToken ct)
    {
        var result = await _bookings.BookAsync(User.GetUserId(), roomId, date, startHour, ct);
        if (result.IsSuccess)
        {
            TempData["Success"] = $"დაჯავშნილია: {date:dd.MM.yyyy}, {startHour:00}:00–{startHour + 1:00}:00.";
        }
        else
        {
            TempData["Error"] = result.Message;
        }

        return RedirectToAction("Details", "Rooms", new { id = roomId, date = date.ToString("yyyy-MM-dd") });
    }

    // US-06
    [HttpGet]
    public async Task<IActionResult> My(CancellationToken ct) =>
        View(await _bookings.GetMyBookingsAsync(User.GetUserId(), ct));

    // US-07
    [HttpPost]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var result = await _bookings.CancelAsync(User.GetUserId(), id, ct);
        if (result.IsSuccess)
        {
            TempData["Success"] = "ჯავშანი გაუქმდა.";
        }
        else
        {
            TempData["Error"] = result.Message;
        }

        return RedirectToAction(nameof(My));
    }
}
