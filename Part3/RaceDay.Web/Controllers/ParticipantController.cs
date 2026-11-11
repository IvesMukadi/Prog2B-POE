using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[RoleRequired("Participant")]
public class ParticipantController : BaseController
{
    private readonly ApiClient _api;

    public ParticipantController(ApiClient api)
    {
        _api = api;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        if (categoryId == 0)
        {
            TempData["Error"] = "Please choose a category first.";
            return RedirectToAction("Details", "Home", new { id = eventId });
        }

        var result = await _api.EnrolAsync(eventId, categoryId);
        if (!result.Success)
        {
            // The API explains why, for example "already enrolled" or "event is not open"
            if (IsSessionOrServerProblem(result))
            {
                return Fail(result, "Details", "Home");
            }
            TempData["Error"] = result.Message;
            return RedirectToAction("Details", "Home", new { id = eventId });
        }

        TempData["Success"] = "You are enrolled. Your enrolment is shown under My Events.";
        return RedirectToAction("MyEvents");
    }

    public async Task<IActionResult> MyEvents()
    {
        var result = await _api.GetMyEnrolmentsAsync();
        if (!result.Success)
        {
            return Fail(result, "Index", "Home");
        }
        return View(result.Data!.OrderBy(e => e.EventDate).ToList());
    }

    public async Task<IActionResult> MyResults()
    {
        var result = await _api.GetMyResultsAsync();
        if (!result.Success)
        {
            return Fail(result, "Index", "Home");
        }
        return View(result.Data!.OrderByDescending(r => r.EventDate).ToList());
    }
}
