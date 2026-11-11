using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Tests;

public class BannerTests : IDisposable
{
    private readonly RaceDayFactory _factory = new();
    private readonly HttpClient _client;

    public BannerTests()
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
    public async Task UploadBanner_NotLoggedIn_ReturnsUnauthorized()
    {
        var response = await _client.PostAsync("/api/events/1/banner", FormWithFile("banner.png", "image/png"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UploadBanner_Participant_ReturnsForbidden()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        var response = await _client.PostAsync("/api/events/1/banner", FormWithFile("banner.png", "image/png"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UploadBanner_OtherOrganisersEvent_ReturnsForbidden()
    {
        // Event 1 belongs to Thabo
        await TestHelper.LoginAs(_client, TestHelper.Organiser2);

        var response = await _client.PostAsync("/api/events/1/banner", FormWithFile("banner.png", "image/png"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UploadBanner_EventNotFound_ReturnsNotFound()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsync("/api/events/999/banner", FormWithFile("banner.png", "image/png"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UploadBanner_WrongFileType_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsync("/api/events/1/banner", FormWithFile("notes.txt", "text/plain"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadBanner_TooBig_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        // 3 MB is over the 2 MB limit
        var response = await _client.PostAsync("/api/events/1/banner", FormWithFile("banner.png", "image/png", 3 * 1024 * 1024));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadBanner_NoFile_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsync("/api/events/1/banner", new MultipartFormDataContent());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadBanner_ValidImage_SavesTheUrl()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsync("/api/events/1/banner", FormWithFile("banner.png", "image/png"));
        var updated = await response.Content.ReadFromJsonAsync<EventDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("https://fake.blob/banners/", updated!.BannerUrl);

        // The event keeps the link
        var again = await _client.GetFromJsonAsync<EventDto>("/api/events/1");
        Assert.Equal(updated.BannerUrl, again!.BannerUrl);
    }
}
