using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Web.Filters;

// Put this on a controller or action to limit it to some roles
public class RoleRequiredAttribute : ActionFilterAttribute
{
    private readonly string[] _roles;

    public RoleRequiredAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var role = context.HttpContext.Session.GetString("Role");

        // Not logged in, so go to the login page
        if (role == null)
        {
            if (context.Controller is Controller controller)
            {
                controller.TempData["Error"] = "Please log in to see that page.";
            }
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        // Logged in with the wrong role
        if (!_roles.Contains(role))
        {
            context.Result = new RedirectToActionResult("Denied", "Account", null);
        }
    }
}
