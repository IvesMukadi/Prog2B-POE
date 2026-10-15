using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Api.Tests;

public class AuthTests : IDisposable
{
    private readonly RaceDayFactory _factory = new();
    private readonly HttpClient _client;

    public AuthTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Register_WithValidDetails_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "new.runner@example.com",
            password = "Secret123",
            firstName = "New",
            lastName = "Runner",
            role = "Participant"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        // This email is already in the seed data
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = TestHelper.Participant1,
            password = "Secret123",
            firstName = "Copy",
            lastName = "Cat",
            role = "Participant"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidRole_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "admin.try@example.com",
            password = "Secret123",
            firstName = "Admin",
            lastName = "Try",
            role = "Admin"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithBadEmail_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "not-an-email",
            password = "Secret123",
            firstName = "Bad",
            lastName = "Email",
            role = "Participant"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
