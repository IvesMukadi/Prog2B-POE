using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Tests;

public class EventsTests : IDisposable
{
    private readonly RaceDayFactory _factory = new();
    private readonly HttpClient _client;

    public EventsTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // A valid event body used by the create tests
    private static object NewEvent()
    {
        return new
        {
            name = "Soweto 10km",
            description = "A fun run through Soweto",
            date = "2027-03-20T07:00:00",
            location = "Orlando Stadium, Soweto",
            distance = 10.0,
            eventTypeId = 1
        };
    }

    [Fact]
    public async Task GetEvents_NotLoggedIn_ReturnsAllEvents()
    {
        var response = await _client.GetAsync("/api/events");
        var events = await response.Content.ReadFromJsonAsync<List<EventDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, events!.Count);
    }

    [Fact]
    public async Task GetEvents_FilterByStatus_ReturnsOnlyThatStatus()
    {
        // Status 4 is Completed
        var response = await _client.GetAsync("/api/events?status=4");
        var events = await response.Content.ReadFromJsonAsync<List<EventDto>>();

        Assert.Single(events!);
        Assert.Equal("Completed", events![0].EventStatus);
    }

    [Fact]
    public async Task GetEvent_ReturnsEventWithCategories()
    {
        var response = await _client.GetAsync("/api/events/1");
        var ev = await response.Content.ReadFromJsonAsync<EventDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Run", ev!.EventType);
        Assert.Equal(4, ev.Categories!.Count);
    }

    [Fact]
    public async Task GetEvent_UnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/events/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_AsOrganiser_ReturnsCreated()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsJsonAsync("/api/events", NewEvent());
        var ev = await response.Content.ReadFromJsonAsync<EventDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Soweto 10km", ev!.Name);
        Assert.Equal("Thabo Mokoena", ev.OrganiserName);
    }

    [Fact]
    public async Task CreateEvent_AsParticipant_ReturnsForbidden()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        var response = await _client.PostAsJsonAsync("/api/events", NewEvent());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_NotLoggedIn_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/events", NewEvent());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_WithUnknownType_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsJsonAsync("/api/events", new
        {
            name = "Mystery Race",
            description = "Unknown type",
            date = "2027-03-20T07:00:00",
            location = "Somewhere",
            distance = 5.0,
            eventTypeId = 99
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEvent_AsOwner_ReturnsUpdatedEvent()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PutAsJsonAsync("/api/events/1", new
        {
            name = "Cape Town Marathon 2027 (Updated)",
            description = "New description",
            date = "2027-05-16T06:00:00",
            location = "Cape Town Stadium, Cape Town",
            distance = 42.2,
            eventTypeId = 1,
            eventStatusId = 3
        });
        var ev = await response.Content.ReadFromJsonAsync<EventDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Cape Town Marathon 2027 (Updated)", ev!.Name);
        Assert.Equal("Closed", ev.EventStatus);
    }

    [Fact]
    public async Task UpdateEvent_AsOtherOrganiser_ReturnsForbidden()
    {
        // Event 1 belongs to Thabo, not Priya
        await TestHelper.LoginAs(_client, TestHelper.Organiser2);

        var response = await _client.PutAsJsonAsync("/api/events/1", new
        {
            name = "Hijacked",
            description = "Not mine",
            date = "2027-05-16T06:00:00",
            location = "Nowhere",
            distance = 1.0,
            eventTypeId = 1,
            eventStatusId = 2
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEvent_AsOwner_ReturnsNoContentAndEventIsGone()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.DeleteAsync("/api/events/1");
        var after = await _client.GetAsync("/api/events/1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, after.StatusCode);
    }

    [Fact]
    public async Task DeleteEvent_AsParticipant_ReturnsForbidden()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        var response = await _client.DeleteAsync("/api/events/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMyEvents_AsOrganiser_ReturnsOnlyOwnEvents()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.GetAsync("/api/events/mine");
        var events = await response.Content.ReadFromJsonAsync<List<EventDto>>();

        Assert.Equal(2, events!.Count);
        Assert.All(events, e => Assert.Equal("Thabo Mokoena", e.OrganiserName));
    }
}
