using System.Diagnostics;
using BookSpace.Core.Services;
using BookSpace.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookSpace.Web.Controllers;

public class HomeController : Controller
{
    private readonly RoomService _rooms;

    public HomeController(RoomService rooms)
    {
        _rooms = rooms;
    }

    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(new HomeViewModel { Rooms = await _rooms.GetActiveRoomsAsync(ct) });

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
