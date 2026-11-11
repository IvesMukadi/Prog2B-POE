using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Tests;

public class PictureTests : IDisposable
{
    private readonly RaceDayFactory _factory = new();
    private readonly HttpClient _client;

    public PictureTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // Builds a form with a fake file of the given size
    private static MultipartFormDataContent FormWithFile(string fileName, string contentType, int size = 100)
    {
        var file = new ByteArrayContent(new byte[size]);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        var form = new MultipartFormDataContent();
        form.Add(file, "file", fileName);
        return form;
    }

    [Fact]
    public async Task UploadPicture_NotLoggedIn_ReturnsUnauthorized()
    {
        var response = await _client.PostAsync("/api/users/me/picture", FormWithFile("me.jpg", "image/jpeg"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UploadPicture_WrongFileType_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        var response = await _client.PostAsync("/api/users/me/picture", FormWithFile("me.gif", "image/gif"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadPicture_TooBig_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        // 3 MB is over the 2 MB limit
        var response = await _client.PostAsync("/api/users/me/picture", FormWithFile("me.jpg", "image/jpeg", 3 * 1024 * 1024));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadPicture_Participant_SavesTheUrl()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        var response = await _client.PostAsync("/api/users/me/picture", FormWithFile("me.jpg", "image/jpeg"));
        var user = await response.Content.ReadFromJsonAsync<UserDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("https://fake.blob/profiles/", user!.ProfilePictureUrl);

        // The profile keeps the link
        var again = await _client.GetFromJsonAsync<UserDto>("/api/users/me");
        Assert.Equal(user.ProfilePictureUrl, again!.ProfilePictureUrl);
    }

    [Fact]
    public async Task UploadPicture_Organiser_AlsoWorks()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsync("/api/users/me/picture", FormWithFile("me.png", "image/png"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
