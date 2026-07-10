using Balenthiran.Habits.Abstractions.Views;

namespace Balenthiran.Habits.Abstractions.Services;

/// <summary>
/// The daily loop (design MVP 2): assemble a day's view with completion + streaks,
/// upsert a single entry, and produce a habit's history grid.
/// </summary>
public interface IDayService
{
    /// <summary>Every active habit for <paramref name="date"/> with its entry value, completion and streak.</summary>
    Task<DayView> GetDayAsync(DateOnly date);

    /// <summary>Idempotent upsert of one habit's value on one date. Throws if the habit does not exist.</summary>
    Task<HabitDayView> UpsertEntryAsync(int habitId, DateOnly date, double value);

    /// <summary>Last <paramref name="days"/> days (ending <paramref name="today"/>) plus current/longest streak.</summary>
    Task<HabitHistory?> GetHistoryAsync(int habitId, DateOnly today, int days = 30);
}
