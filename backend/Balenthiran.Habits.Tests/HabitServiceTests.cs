using Balenthiran.Habits.Abstractions.Enums;
using Balenthiran.Habits.Abstractions.Views;
using Balenthiran.Habits.Database;
using Balenthiran.Habits.Services;
using Microsoft.EntityFrameworkCore;

namespace Balenthiran.Habits.Tests;

/// <summary>
/// Habit-list management against an in-memory database: create appends to the order,
/// archive is a soft delete that hides but preserves, reorder rewrites positions, and
/// starter seeding runs once.
/// </summary>
public class HabitServiceTests
{
    private static AppDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"habit-{Guid.NewGuid()}")
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Create_appends_to_the_end_of_the_order()
    {
        using var db = NewDb();
        var svc = new HabitService(db);

        var first = await svc.CreateAsync(new HabitInput("Read", HabitType.Boolean));
        var second = await svc.CreateAsync(new HabitInput("Sleep", HabitType.Numeric, "h", 8));

        Assert.Equal(0, first.SortOrder);
        Assert.Equal(1, second.SortOrder);
        Assert.Equal(8, second.Target);
        Assert.Equal("h", second.Unit);
    }

    [Fact]
    public async Task GetAll_excludes_archived_unless_asked()
    {
        using var db = NewDb();
        var svc = new HabitService(db);
        var keep = await svc.CreateAsync(new HabitInput("Keep", HabitType.Boolean));
        var drop = await svc.CreateAsync(new HabitInput("Drop", HabitType.Boolean));

        await svc.ArchiveAsync(drop.Id);

        Assert.Equal(new[] { "Keep" }, (await svc.GetAllAsync()).Select(h => h.Name));
        Assert.Equal(2, (await svc.GetAllAsync(includeArchived: true)).Count);
        // Archiving preserves the row (history survives) rather than deleting it.
        Assert.NotNull(await svc.GetAsync(drop.Id));
    }

    [Fact]
    public async Task Update_changes_fields_and_returns_null_when_missing()
    {
        using var db = NewDb();
        var svc = new HabitService(db);
        var habit = await svc.CreateAsync(new HabitInput("Wtaer", HabitType.Numeric, "glass", 6));

        var updated = await svc.UpdateAsync(habit.Id, new HabitInput("Water", HabitType.Numeric, "glasses", 8));

        Assert.NotNull(updated);
        Assert.Equal("Water", updated!.Name);
        Assert.Equal("glasses", updated.Unit);
        Assert.Equal(8, updated.Target);
        Assert.Null(await svc.UpdateAsync(999, new HabitInput("Nope", HabitType.Boolean)));
    }

    [Fact]
    public async Task Reorder_rewrites_sort_positions()
    {
        using var db = NewDb();
        var svc = new HabitService(db);
        var a = await svc.CreateAsync(new HabitInput("A", HabitType.Boolean));
        var b = await svc.CreateAsync(new HabitInput("B", HabitType.Boolean));
        var c = await svc.CreateAsync(new HabitInput("C", HabitType.Boolean));

        await svc.ReorderAsync([c.Id, a.Id, b.Id]);

        Assert.Equal(new[] { "C", "A", "B" }, (await svc.GetAllAsync()).Select(h => h.Name));
    }

    [Fact]
    public async Task Archive_returns_false_for_an_unknown_habit()
    {
        using var db = NewDb();
        var svc = new HabitService(db);
        Assert.False(await svc.ArchiveAsync(999));
    }

    [Fact]
    public async Task EnsureSeeded_creates_the_starter_set_once()
    {
        using var db = NewDb();
        var svc = new HabitService(db);

        var seeded = await svc.EnsureSeededAsync();
        Assert.Equal(StarterHabits.Build().Count, seeded.Count);
        Assert.Equal(new[] { "Sleep", "Exercise", "Read", "Water" }, seeded.Select(h => h.Name));

        // Second call is a no-op — never duplicates the starter set.
        var again = await svc.EnsureSeededAsync();
        Assert.Empty(again);
        Assert.Equal(StarterHabits.Build().Count, await db.Habits.CountAsync());
    }
}
