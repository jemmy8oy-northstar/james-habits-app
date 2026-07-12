using Scalar.AspNetCore;
using Balenthiran.Habits.WebApi;
using Balenthiran.Habits.WebApi.ExceptionHandling;
using Balenthiran.Habits.WebApi.Routes;
using Balenthiran.Habits.Abstractions.Services;
using Balenthiran.Habits.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBackendServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Global exception handling (issue #7): AppExceptionHandler maps thrown AppExceptions to
// RFC 7807 ProblemDetails; AddProblemDetails supplies the writer + standard fields.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<AppExceptionHandler>();

var app = builder.Build();

// Must sit at the top of the pipeline so it catches exceptions from everything below it.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/scalar/v1");
}

// When the build-time OpenAPI generator (GetDocument.Insider) loads the app purely to emit the
// OpenAPI document it runs this top-level code but never serves requests — it must not touch a
// database, or a Debug build would try to migrate against whatever connection string is configured.
var generatingOpenApiDocument =
    System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";

if (!generatingOpenApiDocument)
{
    using var scope = app.Services.CreateScope();
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
