using Scalar.AspNetCore;
using Balenthiran.Habits.WebApi;
using Balenthiran.Habits.WebApi.Routes;
using Balenthiran.Habits.Abstractions.Services;
using Balenthiran.Habits.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBackendServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/scalar/v1");
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
    if (dbContext is null)
    {
        app.Logger.LogWarning("Skipping database migration — no connection string configured.");
    }
    else
    {
        // Relational providers migrate; a non-relational provider (the in-memory store used
        // by integration tests) just materialises its schema. Either way we then seed.
        if (dbContext.Database.IsRelational())
        {
            dbContext.Database.Migrate();
        }
        else
        {
            dbContext.Database.EnsureCreated();
        }

        // Seed the starter habit set on first run so the app is never empty (design A5).
        await scope.ServiceProvider.GetRequiredService<IHabitService>().EnsureSeededAsync();
    }
}

app.UseHttpsRedirection();

app.MapGroup("/api")
    .MapStatusRoutes()
    .MapHabitRoutes()
    .MapDayRoutes()
    .WithOpenApi();

app.Run();

/// <summary>Exposed so the integration test host (WebApplicationFactory) can boot the API.</summary>
public partial class Program;
