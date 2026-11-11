using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Api.Services;

namespace RaceDay.Api.Tests;

public class RaceDayFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Swap SQL Server for an in-memory database so tests need no server
            var oldOptions = services.Single(d => d.ServiceType == typeof(DbContextOptions<RaceDayContext>));
            services.Remove(oldOptions);

            // A new database name for every test keeps the tests separate
            var dbName = Guid.NewGuid().ToString();
            services.AddDbContext<RaceDayContext>(options => options.UseInMemoryDatabase(dbName));

            // The last registration wins, so the tests use the fake blob service
            services.AddScoped<IBlobService, FakeBlobService>();
        });
    }
}
