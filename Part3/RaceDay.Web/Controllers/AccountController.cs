using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class AccountController : BaseController
{
    private readonly ApiClient _api;

    public AccountController(ApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Login()
    {
        // Already logged in, so go to the right home page
        if (HttpContext.Session.GetString("Role") != null)
        {
            return RedirectToHome();
        }
        return View(new LoginForm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginForm form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var result = await _api.LoginAsync(form.Email, form.Password);
        if (result.StatusCode == 0)
        {
            return View("ApiDown");
        }
        if (!result.Success)
        {
            // Shows "Invalid email or password." from the API
            ModelState.AddModelError("", result.Message);
            return View(form);
        }

        SaveUser(result.Data!);
        return RedirectToHome();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterForm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterForm form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var result = await _api.RegisterAsync(form);
        if (result.StatusCode == 0)
        {
            return View("ApiDown");
        }
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message);
            return View(form);
        }

        // Log the new user straight in
        var login = await _api.LoginAsync(form.Email, form.Password);
        if (!login.Success)
        {
            TempData["Success"] = "Account created. Please log in.";
            return RedirectToAction("Login");
        }

        SaveUser(login.Data!);
        return RedirectToHome();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _api.LogoutAsync();
        HttpContext.Session.Clear();
        TempData["Success"] = "You have been logged out.";
        return RedirectToAction("Login");
    }

    public IActionResult Denied()
    {
        return View();
    }

    // Puts the user's details in the MVC session
    private void SaveUser(UserInfo user)
    {
        HttpContext.Session.SetString("UserId", user.UserId.ToString());
        HttpContext.Session.SetString("Name", user.FirstName + " " + user.LastName);
        HttpContext.Session.SetString("Role", user.Role);
        HttpContext.Session.SetString("Picture", user.ProfilePictureUrl ?? "");
    }

    // Organisers start on the dashboard, participants on the home page
    private IActionResult RedirectToHome()
    {
        if (HttpContext.Session.GetString("Role") == "Organiser")
        {
            return RedirectToAction("Dashboard", "Organiser");
        }
        return RedirectToAction("Index", "Home");
    }
}
