using Balenthiran.Habits.Abstractions.Services;
using Balenthiran.Habits.Services;
using Balenthiran.Habits.Database;
using Microsoft.EntityFrameworkCore;

namespace Balenthiran.Habits.WebApi;

public static class ServiceRegistration
{
    public static void AddBackendServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("[WARNING] No database connection string configured — database features are disabled.");
        }
        else
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString, b => b.MigrationsAssembly("Balenthiran.Habits.Database")));
        }

        services.AddAutoMapper(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));
        services.AddScoped<IStatusService, StatusService>();
        services.AddScoped<IHabitService, HabitService>();
        services.AddScoped<IDayService, DayService>();
    }
}
