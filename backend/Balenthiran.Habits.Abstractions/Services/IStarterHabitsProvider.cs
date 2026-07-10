using Balenthiran.Habits.Abstractions.DataModels;

namespace Balenthiran.Habits.Abstractions.Services;

/// <summary>
/// Supplies the sensible starter set seeded on first run so the app is never empty
/// (design A5). Returns plain inputs — the seeder turns them into stored habits — so
/// this stays free of any persistence concern. A DI service rather than a static class.
/// </summary>
public interface IStarterHabitsProvider
{
    /// <summary>The starter habits, in display order. James can rename, reorder or archive any.</summary>
    IReadOnlyList<IHabitInput> GetStarterHabits();
}
