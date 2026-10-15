using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Api.Tests;

// Tests that bad input is rejected with 400
public class ValidationTests : IDisposable
{
    private readonly RaceDayFactory _factory = new();
    private readonly HttpClient _client;

    public ValidationTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Register_WithShortPassword_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "short.pass@example.com",
            password = "123",
            firstName = "Short",
            lastName = "Pass",
            role = "Participant"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithMissingFirstName_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "no.name@example.com",
            password = "Secret123",
            lastName = "Name",
            role = "Participant"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithEmailInDifferentCase_ReturnsBadRequest()
    {
        // The seeded email in capital letters is still the same email
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "AMARA@RACEDAY.CO.ZA",
            password = "Secret123",
            firstName = "Copy",
            lastName = "Cat",
            role = "Participant"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMe_WithEmptyName_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        var response = await _client.PutAsJsonAsync("/api/users/me", new
        {
            email = TestHelper.Participant1,
            firstName = "",
            lastName = "Williams"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_WithZeroDistance_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsJsonAsync("/api/events", new
        {
            name = "Zero Run",
            description = "No distance",
            date = "2027-03-20T07:00:00",
            location = "Nowhere",
            distance = 0,
            eventTypeId = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_WithNegativeDistance_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsJsonAsync("/api/events", new
        {
            name = "Backwards Run",
            description = "Negative distance",
            date = "2027-03-20T07:00:00",
            location = "Nowhere",
            distance = -5,
            eventTypeId = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_WithoutDate_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsJsonAsync("/api/events", new
        {
            name = "No Date Run",
            description = "Date is missing",
            location = "Nowhere",
            distance = 5,
            eventTypeId = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_WithoutName_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsJsonAsync("/api/events", new
        {
            description = "Name is missing",
            date = "2027-03-20T07:00:00",
            location = "Nowhere",
            distance = 5,
            eventTypeId = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEvent_WithUnknownStatus_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PutAsJsonAsync("/api/events/1", new
        {
            name = "Cape Town Marathon 2027",
            description = "Status does not exist",
            date = "2027-05-15T06:00:00",
            location = "Cape Town Stadium, Cape Town",
            distance = 42.2,
            eventTypeId = 1,
            eventStatusId = 99
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddCategory_WithoutName_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsJsonAsync("/api/events/1/categories", new { ageGroup = "18-34" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_ToExistingName_ReturnsBadRequest()
    {
        // Category 1 is renamed to the name of category 2 in the same event
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PutAsJsonAsync("/api/categories/1", new { categoryName = "Elite Men", ageGroup = "18-34" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_WithCategoryIdZero_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        var response = await _client.PostAsJsonAsync("/api/events/1/enrol", new { categoryId = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddResult_WithPositionZero_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsJsonAsync("/api/events/3/results", new
        {
            enrolmentId = 3,
            finishTime = "02:20:10",
            finishingPosition = 0
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddResult_WithBadFinishTime_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsJsonAsync("/api/events/3/results", new
        {
            enrolmentId = 3,
            finishTime = "not a time",
            finishingPosition = 5
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
