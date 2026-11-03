using BookSpace.Core.Abstractions;
using BookSpace.Core.Services;
using BookSpace.Web.Auth;
using BookSpace.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookSpace.Web.Controllers;

// US-11
[Authorize(Roles = Roles.AdminOrManager)]
public class StatisticsController : Controller
{
    private readonly StatisticsService _statistics;
    private readonly IClock _clock;

    public StatisticsController(StatisticsService statistics, IClock clock)
    {
        _statistics = statistics;
        _clock = clock;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        var today = _clock.Today;
        var model = new StatisticsViewModel
        {
            From = from ?? today.AddDays(-30),
            To = to ?? today
        };

        var result = await _statistics.GetRoomUsageAsync(model.From, model.To, ct);
        if (result.IsSuccess)
        {
            model.Usage = result.Value!;
        }
        else
        {
            model.Error = result.Message;
        }

        return View(model);
    }
}
