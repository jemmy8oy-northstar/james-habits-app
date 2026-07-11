using Balenthiran.Habits.Abstractions.DataModels;
using Balenthiran.Habits.Abstractions.Services;
using Balenthiran.Habits.Database;
using Balenthiran.Habits.DataModels.Models;
using Balenthiran.Habits.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace Balenthiran.Habits.Services;

/// <summary>
/// The daily loop: assemble a date's view (habits + entries + streaks), upsert a
/// single entry, and build a habit's history grid. Completion/streak maths is
/// delegated to the injected <see cref="IHabitCalculator"/>.
/// </summary>
public class DayService(AppDbContext db, IHabitCalculator calculator) : IDayService
{
    public async Task<IDayView> GetDayAsync(DateOnly date)
    {
        var habits = await db.Habits
            .Where(h => !h.IsArchived)
            .OrderBy(h => h.SortOrder)
            .ThenBy(h => h.Id)
            .ToListAsync();

        var habitIds = habits.Select(h => h.Id).ToList();

        // All entries for the active habits — streak counts back over full history.
        var entries = await db.HabitEntries
            .Where(e => habitIds.Contains(e.HabitId))
            .ToListAsync();
        var byHabit = entries.ToLookup(e => e.HabitId);

        var views = habits
            .Select(h => BuildDayView(h, byHabit[h.Id], date))
            .ToList();

        return new DayView(date, views);
    }

    public async Task<IHabitDayView> UpsertEntryAsync(int habitId, DateOnly date, double value)
    {
        var habit = await db.Habits.FindAsync(habitId)
            ?? throw new KeyNotFoundException($"Habit {habitId} does not exist.");

        var entry = await db.HabitEntries
            .FirstOrDefaultAsync(e => e.HabitId == habitId && e.Date == date);

        if (entry is null)
        {
            entry = new HabitEntryEntity { HabitId = habitId, Date = date, Value = value, UpdatedAt = DateTime.UtcNow };
            db.HabitEntries.Add(entry);
        }
        else
        {
            entry.Value = value;
            entry.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();

        // Recompute the habit's slot as of the edited date.
        var entries = await db.HabitEntries
            .Where(e => e.HabitId == habitId)
            .ToListAsync();
        return BuildDayView(habit, entries, date);
    }

    public async Task<IHabitHistory?> GetHistoryAsync(int habitId, DateOnly today, int days = 30)
    {
        var habit = await db.Habits.FindAsync(habitId);
        if (habit is null)
            return null;

        var entries = await db.HabitEntries
            .Where(e => e.HabitId == habitId)
            .ToListAsync();

        var valueByDate = entries.ToDictionary(e => e.Date, e => e.Value);
        var completeDates = CompleteDates(habit, entries);

        var grid = new List<DayCompletion>(days);
        for (var i = days - 1; i >= 0; i--)
        {
            var d = today.AddDays(-i);
            grid.Add(new DayCompletion(
                d,
                completeDates.Contains(d),
                valueByDate.TryGetValue(d, out var v) ? v : null));
        }

        return new HabitHistory(
            habit.Id,
            habit.Name,
            calculator.CurrentStreak(completeDates, today),
            calculator.LongestStreak(completeDates),
            grid);
    }

    private HabitDayView BuildDayView(HabitEntity habit, IEnumerable<HabitEntryEntity> entries, DateOnly date)
    {
        var list = entries as ICollection<HabitEntryEntity> ?? entries.ToList();
        var completeDates = CompleteDates(habit, list);
        var todays = list.FirstOrDefault(e => e.Date == date);

        return new HabitDayView(
            habit.Id,
            habit.Name,
            habit.Type,
            habit.Unit,
            habit.Target,
            habit.SortOrder,
            todays?.Value,
            todays is not null && calculator.IsComplete(habit.Type, habit.Target, todays.Value),
            calculator.CurrentStreak(completeDates, date));
    }

    private HashSet<DateOnly> CompleteDates(HabitEntity habit, IEnumerable<HabitEntryEntity> entries) =>
        entries
            .Where(e => calculator.IsComplete(habit.Type, habit.Target, e.Value))
            .Select(e => e.Date)
            .ToHashSet();
}
