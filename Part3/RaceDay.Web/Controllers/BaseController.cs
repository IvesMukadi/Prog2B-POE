using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class BaseController : Controller
{
    // Handles a failed API call: down page, back to login, or back to a page with a message
    protected IActionResult Fail(ApiResult result, string action, string controller)
    {
        if (result.StatusCode == 0)
        {
            return View("ApiDown");
        }

        if (result.StatusCode == 401)
        {
            HttpContext.Session.Clear();
            TempData["Error"] = "Your session ended, please log in again.";
            return RedirectToAction("Login", "Account");
        }

        TempData["Error"] = result.Message;
        return RedirectToAction(action, controller);
    }

    // True when a failed call cannot be fixed by the user retyping a form
    protected static bool IsSessionOrServerProblem(ApiResult result)
    {
        return result.StatusCode == 0 || result.StatusCode == 401;
    }
}
