using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Tests;

public class CategoriesTests : IDisposable
{
    private readonly RaceDayFactory _factory = new();
    private readonly HttpClient _client;

    public CategoriesTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetCategories_NotLoggedIn_ReturnsCategories()
    {
        var response = await _client.GetAsync("/api/events/1/categories");
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(4, categories!.Count);
    }

    [Fact]
    public async Task GetCategories_UnknownEvent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/events/999/categories");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddCategory_AsOwner_ReturnsCreated()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsJsonAsync("/api/events/1/categories", new { categoryName = "Under 20", ageGroup = "Under 20" });
        var category = await response.Content.ReadFromJsonAsync<CategoryDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Under 20", category!.CategoryName);
    }

    [Fact]
    public async Task AddCategory_WithDuplicateName_ReturnsBadRequest()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PostAsJsonAsync("/api/events/1/categories", new { categoryName = "Elite Women", ageGroup = "18-34" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddCategory_AsOtherOrganiser_ReturnsForbidden()
    {
        // Event 1 belongs to Thabo, not Priya
        await TestHelper.LoginAs(_client, TestHelper.Organiser2);

        var response = await _client.PostAsJsonAsync("/api/events/1/categories", new { categoryName = "Sneaky", ageGroup = "All" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AddCategory_AsParticipant_ReturnsForbidden()
    {
        await TestHelper.LoginAs(_client, TestHelper.Participant1);

        var response = await _client.PostAsJsonAsync("/api/events/1/categories", new { categoryName = "Sneaky", ageGroup = "All" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_AsOwner_ReturnsUpdatedCategory()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.PutAsJsonAsync("/api/categories/1", new { categoryName = "Elite Women 18-34", ageGroup = "18-34" });
        var category = await response.Content.ReadFromJsonAsync<CategoryDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Elite Women 18-34", category!.CategoryName);
    }

    [Fact]
    public async Task DeleteCategory_WithoutEnrolments_ReturnsNoContent()
    {
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.DeleteAsync("/api/categories/1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WithEnrolments_ReturnsBadRequest()
    {
        // Category 8 has enrolments in the seed data
        await TestHelper.LoginAs(_client, TestHelper.Organiser1);

        var response = await _client.DeleteAsync("/api/categories/8");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
