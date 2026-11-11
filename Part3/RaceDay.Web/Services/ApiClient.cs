using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Services;

// All calls to the RaceDay API go through this class
public class ApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _accessor;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ApiClient(HttpClient http, IHttpContextAccessor accessor)
    {
        _http = http;
        _accessor = accessor;
    }

    // ---------- Account ----------

    public async Task<ApiResult<UserInfo>> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/auth/login", new { email, password });
            var result = await ReadResponse<UserInfo>(response);

            // Keep the API session cookie (name=value only) in the MVC session
            if (result.Success && response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                var sessionCookie = cookies.FirstOrDefault(c => c.StartsWith("RaceDay.Session="));
                if (sessionCookie != null)
                {
                    _accessor.HttpContext!.Session.SetString("ApiCookie", sessionCookie.Split(';')[0]);
                }
            }

            return result;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new ApiResult<UserInfo> { StatusCode = 0, Message = "Can't reach the server." };
        }
    }

    public Task<ApiResult<UserInfo>> RegisterAsync(RegisterForm form) =>
        Send<UserInfo>(HttpMethod.Post, "/api/auth/register", form);

    public Task<ApiResult<object>> LogoutAsync() =>
        Send<object>(HttpMethod.Post, "/api/auth/logout");

    public Task<ApiResult<UserInfo>> GetMeAsync() =>
        Send<UserInfo>(HttpMethod.Get, "/api/users/me");

    public Task<ApiResult<UserInfo>> UpdateMeAsync(ProfileForm form) =>
        Send<UserInfo>(HttpMethod.Put, "/api/users/me", form);

    public Task<ApiResult<UserInfo>> UploadPictureAsync(IFormFile file) =>
        Send<UserInfo>(HttpMethod.Post, "/api/users/me/picture", null, FileContent(file));

    // ---------- Events ----------

    public Task<ApiResult<List<EventInfo>>> GetEventsAsync(int? type = null, int? status = null)
    {
        var url = "/api/events?";
        if (type != null) url += $"type={type}&";
        if (status != null) url += $"status={status}&";
        return Send<List<EventInfo>>(HttpMethod.Get, url);
    }

    public Task<ApiResult<EventInfo>> GetEventAsync(int id) =>
        Send<EventInfo>(HttpMethod.Get, $"/api/events/{id}");

    public Task<ApiResult<List<EventInfo>>> GetMyEventsAsync() =>
        Send<List<EventInfo>>(HttpMethod.Get, "/api/events/mine");

    public Task<ApiResult<EventInfo>> CreateEventAsync(EventForm form) =>
        Send<EventInfo>(HttpMethod.Post, "/api/events", EventBody(form));

    public Task<ApiResult<EventInfo>> UpdateEventAsync(int id, EventForm form) =>
        Send<EventInfo>(HttpMethod.Put, $"/api/events/{id}", EventBody(form));

    public Task<ApiResult<object>> DeleteEventAsync(int id) =>
        Send<object>(HttpMethod.Delete, $"/api/events/{id}");

    public Task<ApiResult<EventInfo>> UploadBannerAsync(int id, IFormFile file) =>
        Send<EventInfo>(HttpMethod.Post, $"/api/events/{id}/banner", null, FileContent(file));

    // ---------- Categories ----------

    public Task<ApiResult<CategoryInfo>> CreateCategoryAsync(int eventId, CategoryForm form) =>
        Send<CategoryInfo>(HttpMethod.Post, $"/api/events/{eventId}/categories", new { form.CategoryName, form.AgeGroup });

    public Task<ApiResult<CategoryInfo>> UpdateCategoryAsync(int id, CategoryForm form) =>
        Send<CategoryInfo>(HttpMethod.Put, $"/api/categories/{id}", new { form.CategoryName, form.AgeGroup });

    public Task<ApiResult<object>> DeleteCategoryAsync(int id) =>
        Send<object>(HttpMethod.Delete, $"/api/categories/{id}");

    // ---------- Enrolments ----------

    public Task<ApiResult<EnrolmentInfo>> EnrolAsync(int eventId, int categoryId) =>
        Send<EnrolmentInfo>(HttpMethod.Post, $"/api/events/{eventId}/enrol", new { categoryId });

    public Task<ApiResult<List<EnrolmentInfo>>> GetMyEnrolmentsAsync() =>
        Send<List<EnrolmentInfo>>(HttpMethod.Get, "/api/users/me/enrolments");

    public Task<ApiResult<List<EnrolmentInfo>>> GetEventEnrolmentsAsync(int eventId) =>
        Send<List<EnrolmentInfo>>(HttpMethod.Get, $"/api/events/{eventId}/enrolments");

    public Task<ApiResult<EnrolmentInfo>> UpdateEnrolmentStatusAsync(int id, int statusId) =>
        Send<EnrolmentInfo>(HttpMethod.Put, $"/api/enrolments/{id}/status", new { statusId });

    // ---------- Results ----------

    public Task<ApiResult<ResultInfo>> CreateResultAsync(int eventId, int enrolmentId, string finishTime, int position) =>
        Send<ResultInfo>(HttpMethod.Post, $"/api/events/{eventId}/results", new { enrolmentId, finishTime, finishingPosition = position });

    public Task<ApiResult<List<ResultInfo>>> GetEventResultsAsync(int eventId) =>
        Send<List<ResultInfo>>(HttpMethod.Get, $"/api/events/{eventId}/results");

    public Task<ApiResult<List<ResultInfo>>> GetMyResultsAsync() =>
        Send<List<ResultInfo>>(HttpMethod.Get, "/api/users/me/results");

    // ---------- Helpers ----------

    // The event form turned into the body the API expects (create ignores the status)
    private static object EventBody(EventForm form) => new
    {
        form.Name,
        form.Description,
        form.Date,
        form.Location,
        form.Distance,
        form.EventTypeId,
        form.EventStatusId
    };

    // Wraps an uploaded file so it can be sent on to the API
    private static MultipartFormDataContent FileContent(IFormFile file)
    {
        var stream = new StreamContent(file.OpenReadStream());
        stream.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        var form = new MultipartFormDataContent();
        form.Add(stream, "file", file.FileName);
        return form;
    }

    // Sends one request and turns the reply into an ApiResult
    private async Task<ApiResult<T>> Send<T>(HttpMethod method, string url, object? body = null, HttpContent? content = null)
    {
        try
        {
            var request = new HttpRequestMessage(method, url);
            if (body != null) request.Content = JsonContent.Create(body);
            if (content != null) request.Content = content;

            var response = await _http.SendAsync(request);
            return await ReadResponse<T>(response);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // The API is down or too slow
            return new ApiResult<T> { StatusCode = 0, Message = "Can't reach the server." };
        }
    }

    private static async Task<ApiResult<T>> ReadResponse<T>(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        var result = new ApiResult<T>
        {
            Success = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode
        };

        if (result.Success)
        {
            if (text.Length > 0) result.Data = JsonSerializer.Deserialize<T>(text, JsonOptions);
        }
        else
        {
            result.Message = ErrorMessage(response.StatusCode, text);
        }

        return result;
    }

    // Picks the most helpful message for a failed call
    private static string ErrorMessage(HttpStatusCode code, string text)
    {
        // Validation errors come back as JSON with an errors list
        if (text.StartsWith("{"))
        {
            try
            {
                using var doc = JsonDocument.Parse(text);
                if (doc.RootElement.TryGetProperty("errors", out var errors))
                {
                    var messages = errors.EnumerateObject()
                        .SelectMany(p => p.Value.EnumerateArray().Select(m => m.GetString()));
                    return string.Join(" ", messages);
                }
            }
            catch (JsonException)
            {
                // Not the format we expected, so fall through to the default messages
            }
        }
        // Other errors are plain text written by the API
        else if (text.Length > 0)
        {
            return text.Trim('"');
        }

        return code switch
        {
            HttpStatusCode.Unauthorized => "Please log in to continue.",
            HttpStatusCode.Forbidden => "You are not allowed to do that.",
            HttpStatusCode.NotFound => "We could not find that.",
            _ => "Something went wrong. Please try again."
        };
    }
}
