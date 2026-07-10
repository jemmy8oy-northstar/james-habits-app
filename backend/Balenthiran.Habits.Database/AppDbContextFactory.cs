using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Balenthiran.Habits.Database;

/// <summary>
/// Design-time factory used only by the EF Core tools (`dotnet ef migrations …`).
/// Supplies the Npgsql provider so migrations scaffold without a running app or a
/// live database connection. Never used at runtime.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=habits_design;Username=postgres;Password=postgres",
                b => b.MigrationsAssembly("Balenthiran.Habits.Database"))
            .Options;

        return new AppDbContext(options);
    }
}
