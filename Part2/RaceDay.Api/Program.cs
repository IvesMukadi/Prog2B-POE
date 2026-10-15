using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RaceDayContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDay")));

builder.Services.AddControllers();

var app = builder.Build();

// Fill the database with sample data when the app starts
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RaceDayContext>();
    SeedData.Initialize(db);
}

app.MapControllers();

app.Run();

public partial class Program { }
