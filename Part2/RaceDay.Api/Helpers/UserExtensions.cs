using System.Security.Claims;

namespace RaceDay.Api.Helpers;

public static class UserExtensions
{
    // Gets the logged in user's id from the session cookie
    public static int GetUserId(this ClaimsPrincipal user)
    {
        return int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
