using Scalar.AspNetCore;
using Balenthiran.Habits.WebApi;
using Balenthiran.Habits.WebApi.ExceptionHandling;
using Balenthiran.Habits.WebApi.Routes;

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

// Booting the API deliberately does no database work: schema migration and starter-habit
// seeding run out of band (`dotnet ef database update`, see docs/specs/openapi-codegen.md),
// not on startup. So the backend runs — and the build-time OpenAPI generator loads it — with
// no live database, even when a connection string is configured. Integration tests provision
// and seed their own in-memory store (see HabitApiFactory).

app.UseHttpsRedirection();

app.MapGroup("/api")
    .MapStatusRoutes()
    .MapHabitRoutes()
    .MapDayRoutes()
    .WithOpenApi();

app.Run();

/// <summary>Exposed so the integration test host (WebApplicationFactory) can boot the API.</summary>
public partial class Program;
