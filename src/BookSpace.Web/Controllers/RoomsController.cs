using BookSpace.Core.Abstractions;
using BookSpace.Core.Common;
using BookSpace.Core.Rules;
using BookSpace.Core.Services;
using BookSpace.Web.Auth;
using BookSpace.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookSpace.Web.Controllers;

public class RoomsController : Controller
{
    private readonly RoomService _rooms;
    private readonly AvailabilityService _availability;
    private readonly IClock _clock;
    private readonly BookingRules _rules;

    public RoomsController(RoomService rooms, AvailabilityService availability, IClock clock, BookingRules rules)
    {
        _rooms = rooms;
        _availability = availability;
        _clock = clock;
        _rules = rules;
    }

    // US-03
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var rooms = User.IsInRole(Roles.Admin)
            ? await _rooms.GetAllRoomsAsync(ct)
            : await _rooms.GetActiveRoomsAsync(ct);
        return View(rooms);
    }

    // US-04
    [HttpGet]
    public async Task<IActionResult> Details(int id, DateOnly? date, CancellationToken ct)
    {
        var room = await _rooms.GetAsync(id, ct);
        if (room is null || !room.IsActive)
        {
            return NotFound();
        }

        var today = _clock.Today;
        var maxDate = today.AddDays(_rules.MaxDaysAhead);
        var selected = date ?? today;
        if (selected < today) selected = today;
        if (selected > maxDate) selected = maxDate;

        var schedule = await _availability.GetScheduleAsync(id, selected, ct);
        if (!schedule.IsSuccess)
        {
            return NotFound();
        }

        return View(new RoomDetailsViewModel
        {
            Room = room,
            Schedule = schedule.Value!,
            MinDate = today,
            MaxDate = maxDate
        });
    }

    // US-08
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult Create() => View(new RoomFormViewModel());

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(RoomFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _rooms.CreateAsync(User.GetRole(), model.Name, model.Location, model.Capacity, ct);
        if (result.Error == ErrorCode.Forbidden)
        {
            return Forbid();
        }

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["Success"] = $"ოთახი „{result.Value!.Name}“ დაემატა.";
        return RedirectToAction(nameof(Index));
    }

    // US-09
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var room = await _rooms.GetAsync(id, ct);
        return room is null ? NotFound() : View(RoomFormViewModel.From(room));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int id, RoomFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _rooms.UpdateAsync(User.GetRole(), id, model.Name, model.Location, model.Capacity,
            model.IsActive, ct);
        switch (result.Error)
        {
            case ErrorCode.None:
                TempData["Success"] = "ოთახის მონაცემები განახლდა.";
                return RedirectToAction(nameof(Index));
            case ErrorCode.NotFound:
                return NotFound();
            case ErrorCode.Forbidden:
                return Forbid();
            default:
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
        }
    }
}
