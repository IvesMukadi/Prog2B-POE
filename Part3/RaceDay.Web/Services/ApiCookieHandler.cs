namespace RaceDay.Web.Services;

// Adds the API session cookie of the logged in user to every API call
public class ApiCookieHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;

    public ApiCookieHandler(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var cookie = _accessor.HttpContext?.Session.GetString("ApiCookie");
        if (!string.IsNullOrEmpty(cookie))
        {
            request.Headers.Add("Cookie", cookie);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
