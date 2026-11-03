using BookSpace.Core.Common;
using BookSpace.Core.Services;
using BookSpace.Web.Auth;
using BookSpace.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookSpace.Web.Controllers;

public class AccountController : Controller
{
    private readonly AuthService _auth;

    public AccountController(AuthService auth)
    {
        _auth = auth;
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _auth.RegisterAsync(model.Email, model.Password, ct);
        if (!result.IsSuccess)
        {
            var field = result.Error == ErrorCode.Validation && result.Message.Contains("პაროლ")
                ? nameof(model.Password)
                : nameof(model.Email);
            ModelState.AddModelError(field, result.Message);
            return View(model);
        }

        await SignInAsync(result.Value!);
        TempData["Success"] = "ანგარიში შეიქმნა. კეთილი იყოს თქვენი მობრძანება!";
        return RedirectToAction("Index", "Rooms");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _auth.LoginAsync(model.Email, model.Password, ct);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        await SignInAsync(result.Value!);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Rooms");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private Task SignInAsync(BookSpace.Core.Domain.User user) =>
        HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            user.ToPrincipal(CookieAuthenticationDefaults.AuthenticationScheme));
}
