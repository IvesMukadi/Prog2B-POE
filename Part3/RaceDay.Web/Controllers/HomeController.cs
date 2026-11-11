using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class HomeController : BaseController
{
    private readonly ApiClient _api;

    public HomeController(ApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(int? type, string? search, DateTime? from)
    {
        // Only open events (status 2) are shown on the home page
        var result = await _api.GetEventsAsync(type, 2);
        if (result.StatusCode == 0)
        {
            return View("ApiDown");
        }
        if (!result.Success)
        {
            ViewBag.Error = result.Message;
        }

        // Upcoming only, then the name search and date filter
        var events = (result.Data ?? new List<EventInfo>()).Where(e => e.Date >= DateTime.Now).ToList();
        if (!string.IsNullOrWhiteSpace(search))
        {
            events = events.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                                    || e.Location.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (from != null)
        {
            events = events.Where(e => e.Date.Date >= from.Value.Date).ToList();
        }

        // Keep the filter values so the form shows them again
        ViewBag.Type = type;
        ViewBag.Search = search;
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.Next = events.OrderBy(e => e.Date).FirstOrDefault();

        return View(events.OrderBy(e => e.Date).ToList());
    }

    public async Task<IActionResult> Details(int id)
    {
        var result = await _api.GetEventAsync(id);
        if (!result.Success)
        {
            return Fail(result, "Index", "Home");
        }
        return View(result.Data);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
