using Balenthiran.Habits.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Balenthiran.Habits.Tests;

/// <summary>
/// Boots the real WebApi in-process against an isolated in-memory database, so the HTTP
/// routes are exercised end-to-end — routing, model binding, status codes, serialization —
/// without a Postgres server. Each instance gets its own store, so tests stay independent.
/// The app seeds the four starter habits on startup, so a fresh host already has them.
/// </summary>
public sealed class HabitApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"habits-api-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Run outside Development so no local appsettings.Development.json connection string is
        // picked up — the app then skips its Npgsql registration and the in-memory provider below
        // is the only one (a context can hold just one provider).
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_dbName)));
    }
}
