using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Tests;

public class UsersTests : IDisposable
{
    private readonly RaceDayFactory _factory = new();
    private readonly HttpClient _client;

    public UsersTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetMe_NotLoggedIn_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_LoggedIn_ReturnsOwnProfile()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        var response = await _client.GetAsync("/api/users/me");
        var user = await response.Content.ReadFromJsonAsync<UserDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(TestHelper.Participant1, user!.Email);
        Assert.Equal("Participant", user.Role);
    }

    [Fact]
    public async Task UpdateMe_WithNewName_ReturnsUpdatedProfile()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        var response = await _client.PutAsJsonAsync("/api/users/me", new
        {
            email = TestHelper.Participant1,
            firstName = "Amara Grace",
            lastName = "Williams"
        });
        var user = await response.Content.ReadFromJsonAsync<UserDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Amara Grace", user!.FirstName);
    }

    [Fact]
    public async Task UpdateMe_WithEmailOfAnotherUser_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        var response = await _client.PutAsJsonAsync("/api/users/me", new
        {
            email = TestHelper.Participant2,
            firstName = "Amara",
            lastName = "Williams"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
