using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RaceDay.Api.Data;
using RaceDay.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RaceDayContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDay")));

// Uploads go to Azure Blob Storage through this service
builder.Services.AddScoped<IBlobService, BlobService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "RaceDay API", Version = "v1" });

    // Show the summary comments from the controllers in Swagger
    var xmlFile = Path.Combine(AppContext.BaseDirectory, "RaceDay.Api.xml");
    if (File.Exists(xmlFile))
    {
        options.IncludeXmlComments(xmlFile);
    }
});

// Login uses a cookie as the session
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "RaceDay.Session";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);

        // Send 401 and 403 instead of redirecting to a login page
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Fill the database with sample data when the app starts
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RaceDayContext>();
    SeedData.Initialize(db);
}

app.UseSwagger();
app.UseSwaggerUI();

// Authentication must come before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Lets the test project start the app
public partial class Program { }
