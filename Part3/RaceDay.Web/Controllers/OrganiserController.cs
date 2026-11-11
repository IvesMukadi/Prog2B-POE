using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[RoleRequired("Organiser")]
public class OrganiserController : BaseController
{
    private readonly ApiClient _api;

    public OrganiserController(ApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Dashboard()
    {
        var events = await _api.GetMyEventsAsync();
        if (!events.Success)
        {
            return Fail(events, "Index", "Home");
        }

        // One call per event to count the enrolments
        var counts = new Dictionary<int, int>();
        foreach (var ev in events.Data!)
        {
            var enrolments = await _api.GetEventEnrolmentsAsync(ev.EventId);
            if (!enrolments.Success)
            {
                return Fail(enrolments, "Index", "Home");
            }
            counts[ev.EventId] = enrolments.Data!.Count(e => e.StatusId != 3);
        }

        ViewBag.Counts = counts;
        return View(events.Data!.OrderBy(e => e.Date).ToList());
    }
}
