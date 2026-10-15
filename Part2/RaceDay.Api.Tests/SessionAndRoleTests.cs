using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Tests;

// Tests for the session, full role flows and records that do not exist
public class SessionAndRoleTests : IDisposable
{
    private readonly RaceDayFactory _factory = new();
    private readonly HttpClient _client;

    public SessionAndRoleTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Login_WithEmailInCapitals_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "THABO@RACEDAY.CO.ZA",
            password = TestHelper.Password
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AfterLogout_ProtectedEndpoint_ReturnsUnauthorized()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);
        var before = await _client.GetAsync("/api/users/me");

        await _client.PostAsync("/api/auth/logout", null);
        var after = await _client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.OK, before.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, after.StatusCode);
    }

    [Fact]
    public async Task TwoUsers_HaveSeparateSessions()
    {
        // A second client has its own cookies, so it is a different browser
        var otherClient = _factory.CreateClient();
        await TestHelper.LoginAs(_client, TestHelper.Participant1);
        await TestHelper.LoginAs(otherClient, TestHelper.Organiser1);

        var first = await _client.GetFromJsonAsync<UserDto>("/api/users/me");
        var second = await otherClient.GetFromJsonAsync<UserDto>("/api/users/me");

        Assert.Equal("Participant", first!.Role);
        Assert.Equal("Organiser", second!.Role);
    }

    [Fact]
    public async Task NewOrganiser_CanLoginAndCreateEvent()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "new.organiser@example.com",
            password = "Secret123",
            firstName = "New",
            lastName = "Organiser",
            role = "Organiser"
        });
        await _client.PostAsJsonAsync("/api/auth/login", new { email = "new.organiser@example.com", password = "Secret123" });

        var response = await _client.PostAsJsonAsync("/api/events", new
        {
            name = "Brand New 5km",
            description = "Created by a new organiser",
            date = "2027-08-01T08:00:00",
            location = "Local Park",
            distance = 5,
            eventTypeId = 1
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task NewParticipant_CanEnrolButNotCreateEvents()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "new.runner@example.com",
            password = "Secret123",
            firstName = "New",
            lastName = "Runner",
            role = "Participant"
        });
        await _client.PostAsJsonAsync("/api/auth/login", new { email = "new.runner@example.com", password = "Secret123" });

        var enrol = await _client.PostAsJsonAsync("/api/events/1/enrol", new { categoryId = 1 });
        var create = await _client.PostAsJsonAsync("/api/events", new
        {
            name = "Not Allowed",
            description = "Participants cannot do this",
            date = "2027-08-01T08:00:00",
            location = "Local Park",
            distance = 5,
            eventTypeId = 1
        });

        Assert.Equal(HttpStatusCode.Created, enrol.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
    }

    [Fact]
    public async Task Enrol_InClosedEvent_ReturnsBadRequest()
    {
        // The organiser closes event 1, then a participant tries to enrol
        var organiserClient = _factory.CreateClient();
        await TestHelper.LoginAs(organiserClient, TestHelper.Organiser1);
        await organiserClient.PutAsJsonAsync("/api/events/1", new
        {
            name = "Cape Town Marathon 2027",
            description = "Now closed",
            date = "2027-05-15T06:00:00",
            location = "Cape Town Stadium, Cape Town",
            distance = 42.2,
            eventTypeId = 1,
            eventStatusId = 3
        });

        await TestHelper.LoginAs(_client, TestHelper.Participant1);
        var response = await _client.PostAsJsonAsync("/api/events/1/enrol", new { categoryId = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetEvents_FilterByDateWithNoEvents_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/events?date=2030-01-01");
        var events = await response.Content.ReadFromJsonAsync<List<EventDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(events!);
    }

    [Fact]
    public async Task GetEvents_FilterByType_ReturnsOnlyThatType()
    {
        // Type 3 is Cycle
        var events = await _client.GetFromJsonAsync<List<EventDto>>("/api/events?type=3");

        Assert.Single(events!);
        Assert.Equal("Cycle", events![0].EventType);
    }

    [Fact]
    public async Task UpdateEvent_UnknownId_ReturnsNotFound()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PutAsJsonAsync("/api/events/999", new
        {
            name = "Ghost Event",
            description = "Does not exist",
            date = "2027-05-15T06:00:00",
            location = "Nowhere",
            distance = 5,
            eventTypeId = 1,
            eventStatusId = 2
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEvent_UnknownId_ReturnsNotFound()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.DeleteAsync("/api/events/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddCategory_ToUnknownEvent_ReturnsNotFound()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsJsonAsync("/api/events/999/categories", new { categoryName = "Ghost", ageGroup = "All" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_UnknownId_ReturnsNotFound()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PutAsJsonAsync("/api/categories/999", new { categoryName = "Ghost", ageGroup = "All" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_UnknownId_ReturnsNotFound()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.DeleteAsync("/api/categories/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_UnknownEnrolment_ReturnsNotFound()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PutAsJsonAsync("/api/enrolments/999/status", new { statusId = 2 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetEventEnrolments_UnknownEvent_ReturnsNotFound()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.GetAsync("/api/events/999/enrolments");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddResult_ToUnknownEvent_ReturnsNotFound()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsJsonAsync("/api/events/999/results", new
        {
            enrolmentId = 3,
            finishTime = "02:20:10",
            finishingPosition = 5
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMyResults_ForParticipantWithoutResults_ReturnsEmptyList()
    {
        // Lindiwe is enrolled in a completed event but has no result yet
        await TestHelper.LoginAs(_client, TestHelper.Participant3);

        var results = await _client.GetFromJsonAsync<List<ResultDto>>("/api/users/me/results");

        Assert.Empty(results!);
    }
}
