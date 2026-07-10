using Balenthiran.Habits.Abstractions.Enums;
using Balenthiran.Habits.Database;
using Balenthiran.Habits.EntityModels;
using Balenthiran.Habits.Services;
using Microsoft.EntityFrameworkCore;

namespace Balenthiran.Habits.Tests;

/// <summary>
/// The daily loop against an in-memory database: the day view assembles habits in order with
/// their entry/completion/streak, upsert is idempotent, and history walks a fixed window.
/// The streak/completion maths itself lives in <see cref="HabitCalculatorTests"/>.
/// </summary>
public class DayServiceTests
{
    private static readonly DateOnly Today = new(2026, 1, 15);
    private static DateOnly Ago(int days) => Today.AddDays(-days);

    private static AppDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"day-{Guid.NewGuid()}")
            .Options;
        return new AppDbContext(options);
    }

    private static HabitEntity AddHabit(AppDbContext db, string name, HabitType type, double? target = null, int order = 0, bool archived = false)
    {
        var h = new HabitEntity { Name = name, Type = type, Target = target, SortOrder = order, IsArchived = archived };
        db.Habits.Add(h);
        db.SaveChanges();
        return h;
    }

    private static void AddEntries(AppDbContext db, int habitId, params (DateOnly date, double value)[] entries)
    {
        foreach (var (date, value) in entries)
            db.HabitEntries.Add(new HabitEntryEntity { HabitId = habitId, Date = date, Value = value });
        db.SaveChanges();
    }

    [Fact]
    public async Task GetDay_returns_active_habits_in_sort_order()
    {
        using var db = NewDb();
        AddHabit(db, "Second", HabitType.Boolean, order: 1);
        AddHabit(db, "First", HabitType.Boolean, order: 0);
        AddHabit(db, "Archived", HabitType.Boolean, order: 2, archived: true);
        var svc = new DayService(db, new HabitCalculator());

        var day = await svc.GetDayAsync(Today);

        Assert.Equal(new[] { "First", "Second" }, day.Habits.Select(h => h.Name));
        Assert.Equal(Today, day.Date);
    }

    [Fact]
    public async Task GetDay_surfaces_todays_value_and_completion()
    {
        using var db = NewDb();
        var sleep = AddHabit(db, "Sleep", HabitType.Numeric, target: 8);
        AddEntries(db, sleep.Id, (Today, 8.5));
        var svc = new DayService(db, new HabitCalculator());

        var view = (await svc.GetDayAsync(Today)).Habits.Single();

        Assert.Equal(8.5, view.Value);
        Assert.True(view.IsComplete);
    }

    [Fact]
    public async Task GetDay_reports_streak_from_prior_days_even_before_today_is_logged()
    {
        using var db = NewDb();
        var read = AddHabit(db, "Read", HabitType.Boolean);
        // Complete the three days before today; today unlogged (grace day).
        AddEntries(db, read.Id, (Ago(1), 1), (Ago(2), 1), (Ago(3), 1));
        var svc = new DayService(db, new HabitCalculator());

        var view = (await svc.GetDayAsync(Today)).Habits.Single();

        Assert.Null(view.Value);
        Assert.False(view.IsComplete);
        Assert.Equal(3, view.CurrentStreak);
    }

    [Fact]
    public async Task UpsertEntry_inserts_then_overwrites_the_same_day()
    {
        using var db = NewDb();
        var water = AddHabit(db, "Water", HabitType.Numeric, target: 8);
        var svc = new DayService(db, new HabitCalculator());

        var first = await svc.UpsertEntryAsync(water.Id, Today, 5);
        Assert.Equal(5, first.Value);
        Assert.False(first.IsComplete);

        var second = await svc.UpsertEntryAsync(water.Id, Today, 9);
        Assert.Equal(9, second.Value);
        Assert.True(second.IsComplete);

        // Still exactly one row for the day — an upsert, not an append (A2).
        Assert.Equal(1, await db.HabitEntries.CountAsync(e => e.HabitId == water.Id && e.Date == Today));
    }

    [Fact]
    public async Task UpsertEntry_throws_for_an_unknown_habit()
    {
        using var db = NewDb();
        var svc = new DayService(db, new HabitCalculator());
        await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.UpsertEntryAsync(999, Today, 1));
    }

    [Fact]
    public async Task GetHistory_builds_a_fixed_window_with_streaks()
    {
        using var db = NewDb();
        var run = AddHabit(db, "Run", HabitType.Boolean);
        // A 3-day run ending today, plus an earlier isolated complete day.
        AddEntries(db, run.Id, (Today, 1), (Ago(1), 1), (Ago(2), 1), (Ago(5), 1), (Ago(6), 0));
        var svc = new DayService(db, new HabitCalculator());

        var history = await svc.GetHistoryAsync(run.Id, Today, days: 7);

        Assert.NotNull(history);
        Assert.Equal(7, history!.Days.Count);
        Assert.Equal(Ago(6), history.Days.First().Date);
        Assert.Equal(Today, history.Days.Last().Date);
        Assert.Equal(3, history.CurrentStreak);
        Assert.Equal(3, history.LongestStreak);
        // Ago(6) was logged 0 for a boolean → present value, not complete.
        var earliest = history.Days.First();
        Assert.Equal(0, earliest.Value);
        Assert.False(earliest.IsComplete);
    }

    [Fact]
    public async Task GetHistory_returns_null_for_an_unknown_habit()
    {
        using var db = NewDb();
        var svc = new DayService(db, new HabitCalculator());
        Assert.Null(await svc.GetHistoryAsync(999, Today));
    }
}
