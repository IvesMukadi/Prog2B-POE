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

    public async Task<IActionResult> Events()
    {
        var result = await _api.GetMyEventsAsync();
        if (!result.Success)
        {
            return Fail(result, "Dashboard", "Organiser");
        }
        return View(result.Data!.OrderBy(e => e.Date).ToList());
    }

    [HttpGet]
    public IActionResult CreateEvent()
    {
        return View("EventForm", new EventForm { Date = DateTime.Today.AddDays(30).AddHours(7) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(EventForm form, IFormFile? banner)
    {
        if (!ModelState.IsValid)
        {
            return View("EventForm", form);
        }

        var result = await _api.CreateEventAsync(form);
        if (!result.Success)
        {
            if (IsSessionOrServerProblem(result))
            {
                return Fail(result, "Events", "Organiser");
            }
            ModelState.AddModelError("", result.Message);
            return View("EventForm", form);
        }

        // The banner needs the new event id, so it is uploaded after the event is saved
        if (banner != null && banner.Length > 0)
        {
            var upload = await _api.UploadBannerAsync(result.Data!.EventId, banner);
            if (!upload.Success)
            {
                TempData["Error"] = "The event was created, but the banner failed: " + upload.Message;
                return RedirectToAction("EditEvent", new { id = result.Data.EventId });
            }
        }

        TempData["Success"] = "Event created. Add its categories next.";
        return RedirectToAction("Categories", new { id = result.Data!.EventId });
    }

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        var result = await _api.GetEventAsync(id);
        if (!result.Success)
        {
            return Fail(result, "Events", "Organiser");
        }

        var ev = result.Data!;
        ViewBag.Banner = ev.BannerUrl;
        return View("EventForm", new EventForm
        {
            EventId = ev.EventId, Name = ev.Name, Description = ev.Description, Date = ev.Date,
            Location = ev.Location, Distance = ev.Distance, EventTypeId = ev.EventTypeId, EventStatusId = ev.EventStatusId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(int id, EventForm form, IFormFile? banner)
    {
        if (!ModelState.IsValid)
        {
            return View("EventForm", form);
        }

        var result = await _api.UpdateEventAsync(id, form);
        if (!result.Success)
        {
            if (IsSessionOrServerProblem(result))
            {
                return Fail(result, "Events", "Organiser");
            }
            // For example 403 when the event belongs to another organiser
            ModelState.AddModelError("", result.Message);
            return View("EventForm", form);
        }

        if (banner != null && banner.Length > 0)
        {
            var upload = await _api.UploadBannerAsync(id, banner);
            if (!upload.Success)
            {
                TempData["Error"] = "The details were saved, but the banner failed: " + upload.Message;
                return RedirectToAction("EditEvent", new { id });
            }
        }

        TempData["Success"] = "Event saved.";
        return RedirectToAction("Events");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var result = await _api.DeleteEventAsync(id);
        if (!result.Success)
        {
            return Fail(result, "Events", "Organiser");
        }

        TempData["Success"] = "Event deleted.";
        return RedirectToAction("Events");
    }

    public async Task<IActionResult> Categories(int id)
    {
        var result = await _api.GetEventAsync(id);
        if (!result.Success)
        {
            return Fail(result, "Events", "Organiser");
        }

        ViewBag.Event = result.Data;
        return View(new CategoryForm { EventId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(CategoryForm form)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please enter a category name.";
            return RedirectToAction("Categories", new { id = form.EventId });
        }

        var result = await _api.CreateCategoryAsync(form.EventId, form);
        if (!result.Success)
        {
            return Fail(result, "Categories", "Organiser");
        }

        TempData["Success"] = "Category added.";
        return RedirectToAction("Categories", new { id = form.EventId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(CategoryForm form)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please enter a category name.";
            return RedirectToAction("Categories", new { id = form.EventId });
        }

        var result = await _api.UpdateCategoryAsync(form.CategoryId, form);
        if (!result.Success)
        {
            if (IsSessionOrServerProblem(result))
            {
                return Fail(result, "Events", "Organiser");
            }
            TempData["Error"] = result.Message;
            return RedirectToAction("Categories", new { id = form.EventId });
        }

        TempData["Success"] = "Category saved.";
        return RedirectToAction("Categories", new { id = form.EventId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int categoryId, int eventId)
    {
        var result = await _api.DeleteCategoryAsync(categoryId);
        if (!result.Success)
        {
            // The API refuses when people are enrolled, and the message says so
            if (IsSessionOrServerProblem(result))
            {
                return Fail(result, "Events", "Organiser");
            }
            TempData["Error"] = result.Message;
            return RedirectToAction("Categories", new { id = eventId });
        }

        TempData["Success"] = "Category deleted.";
        return RedirectToAction("Categories", new { id = eventId });
    }
}
