using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Tests;

public class ResultsTests : IDisposable
{
    private readonly RaceDayFactory _factory = new();
    private readonly HttpClient _client;

    public ResultsTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task AddResult_AsOwner_ReturnsCreated()
    {
        // Enrolment 3 is Lindiwe's and has no result yet
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsJsonAsync("/api/events/3/results", new
        {
            enrolmentId = 3,
            finishTime = "02:20:10",
            finishingPosition = 71
        });
        var result = await response.Content.ReadFromJsonAsync<ResultDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(71, result!.FinishingPosition);
        Assert.Equal("Lindiwe Zulu", result.ParticipantName);
    }

    [Fact]
    public async Task AddResult_ForEnrolmentWithResult_ReturnsBadRequest()
    {
        // Enrolment 1 already has a result
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsJsonAsync("/api/events/3/results", new
        {
            enrolmentId = 1,
            finishTime = "02:00:00",
            finishingPosition = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddResult_ForEventThatIsNotCompleted_ReturnsBadRequest()
    {
        // Event 1 is still open
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsJsonAsync("/api/events/1/results", new
        {
            enrolmentId = 3,
            finishTime = "02:00:00",
            finishingPosition = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddResult_AsOtherOrganiser_ReturnsForbidden()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser2);

        var response = await _client.PostAsJsonAsync("/api/events/3/results", new
        {
            enrolmentId = 3,
            finishTime = "02:20:10",
            finishingPosition = 71
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AddResult_AsParticipant_ReturnsForbidden()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant3);

        var response = await _client.PostAsJsonAsync("/api/events/3/results", new
        {
            enrolmentId = 3,
            finishTime = "00:10:00",
            finishingPosition = 1
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMyResults_AsParticipant_ReturnsOwnResultWithTotalFinishers()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        var response = await _client.GetAsync("/api/users/me/results");
        var results = await response.Content.ReadFromJsonAsync<List<ResultDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(results!);
        Assert.Equal(34, results![0].FinishingPosition);
        Assert.Equal(2, results[0].TotalFinishers);
        Assert.Equal("Pretoria Night Run 2026", results[0].EventName);
    }

    [Fact]
    public async Task GetMyResults_AsOrganiser_ReturnsForbidden()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.GetAsync("/api/users/me/results");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetEventResults_AsOwner_ReturnsResultsInPositionOrder()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.GetAsync("/api/events/3/results");
        var results = await response.Content.ReadFromJsonAsync<List<ResultDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, results!.Count);
        Assert.Equal(34, results[0].FinishingPosition);
    }

    [Fact]
    public async Task GetEventResults_AsOtherOrganiser_ReturnsForbidden()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser2);

        var response = await _client.GetAsync("/api/events/3/results");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
