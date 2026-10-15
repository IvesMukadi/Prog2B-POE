using System.Net.Http.Json;

namespace RaceDay.Api.Tests;

public static class TestHelper
{
    // Password used by every seeded user
    public const string Password = "Password123!";

    public const string Organiser1 = "thabo@raceday.co.za";
    public const string Organiser2 = "priya@raceday.co.za";
    public const string Participant1 = "amara@raceday.co.za";
    public const string Participant2 = "david@raceday.co.za";
    public const string Participant3 = "lindiwe@raceday.co.za";

    // Logs in so the client keeps the session cookie
    public static async Task LoginAs(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = Password });
        response.EnsureSuccessStatusCode();
    }
}
