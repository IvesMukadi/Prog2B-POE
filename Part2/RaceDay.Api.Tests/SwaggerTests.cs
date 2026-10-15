using System.Net;

namespace RaceDay.Api.Tests;

public class SwaggerTests : IDisposable
{
    private readonly RaceDayFactory _factory = new();

    [Fact]
    public async Task SwaggerJson_IsAvailable()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
