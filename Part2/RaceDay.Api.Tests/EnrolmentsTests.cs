using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Tests;

public class EnrolmentsTests : IDisposable
{
    private readonly RaceDayFactory _factory = new();
    private readonly HttpClient _client;

    public EnrolmentsTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Enrol_AsParticipant_ReturnsCreatedAndRecordsEnrolment()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        var response = await _client.PostAsJsonAsync("/api/events/1/enrol", new { categoryId = 1 });
        var enrolment = await response.Content.ReadFromJsonAsync<EnrolmentDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Pending", enrolment!.StatusName);
        Assert.Equal("Elite Women", enrolment.CategoryName);

        // The enrolment should now show up in the participant's own list
        var mine = await _client.GetFromJsonAsync<List<EnrolmentDto>>("/api/users/me/enrolments");
        Assert.Equal(2, mine!.Count);
        Assert.Contains(mine, e => e.EventId == 1);
    }

    [Fact]
    public async Task Enrol_Twice_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);
        await _client.PostAsJsonAsync("/api/events/1/enrol", new { categoryId = 1 });

        var response = await _client.PostAsJsonAsync("/api/events/1/enrol", new { categoryId = 2 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_WithCategoryFromAnotherEvent_ReturnsBadRequest()
    {
        // Category 5 belongs to event 2, not event 1
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        var response = await _client.PostAsJsonAsync("/api/events/1/enrol", new { categoryId = 5 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_InCompletedEvent_ReturnsBadRequest()
    {
        // Event 3 is Completed so it is not open for enrolment
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        var response = await _client.PostAsJsonAsync("/api/events/3/enrol", new { categoryId = 7 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_AsOrganiser_ReturnsForbidden()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser2);

        var response = await _client.PostAsJsonAsync("/api/events/1/enrol", new { categoryId = 1 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_NotLoggedIn_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/events/1/enrol", new { categoryId = 1 });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_UnknownEvent_ReturnsNotFound()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        var response = await _client.PostAsJsonAsync("/api/events/999/enrol", new { categoryId = 1 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMyEnrolments_AsParticipant_ReturnsOwnEnrolments()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        var response = await _client.GetAsync("/api/users/me/enrolments");
        var enrolments = await response.Content.ReadFromJsonAsync<List<EnrolmentDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(enrolments!);
        Assert.Equal("Pretoria Night Run 2026", enrolments![0].EventName);
    }

    [Fact]
    public async Task GetMyEnrolments_AsOrganiser_ReturnsForbidden()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.GetAsync("/api/users/me/enrolments");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetEventEnrolments_AsOwner_ReturnsParticipantsWithCategories()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.GetAsync("/api/events/3/enrolments");
        var enrolments = await response.Content.ReadFromJsonAsync<List<EnrolmentDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, enrolments!.Count);
        Assert.Contains(enrolments, e => e.ParticipantName == "Amara Williams" && e.CategoryName == "Elite Women");
    }

    [Fact]
    public async Task GetEventEnrolments_AsOtherOrganiser_ReturnsForbidden()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser2);

        var response = await _client.GetAsync("/api/events/3/enrolments");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetEventEnrolments_AsParticipant_ReturnsForbidden()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        var response = await _client.GetAsync("/api/events/3/enrolments");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_AsOwner_ReturnsUpdatedEnrolment()
    {
        // Enrolment 3 is Lindiwe's and starts as Pending
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PutAsJsonAsync("/api/enrolments/3/status", new { statusId = 2 });
        var enrolment = await response.Content.ReadFromJsonAsync<EnrolmentDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Confirmed", enrolment!.StatusName);
    }

    [Fact]
    public async Task UpdateStatus_AsOtherOrganiser_ReturnsForbidden()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser2);

        var response = await _client.PutAsJsonAsync("/api/enrolments/3/status", new { statusId = 2 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_WithUnknownStatus_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PutAsJsonAsync("/api/enrolments/3/status", new { statusId = 99 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
