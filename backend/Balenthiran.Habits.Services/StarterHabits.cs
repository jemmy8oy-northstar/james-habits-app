using Balenthiran.Habits.Abstractions.Enums;
using Balenthiran.Habits.EntityModels;

namespace Balenthiran.Habits.Services;

/// <summary>
/// The sensible starter set seeded on first run so the app is never empty (design A5).
/// James can rename, reorder, add or archive any of these.
/// </summary>
public static class StarterHabits
{
    /// <summary>Fresh entity instances in display order. New each call — never share tracked instances.</summary>
    public static IReadOnlyList<HabitEntity> Build() =>
    [
        new HabitEntity { Name = "Sleep",    Type = HabitType.Numeric, Unit = "h",       Target = 8, SortOrder = 0 },
        new HabitEntity { Name = "Exercise", Type = HabitType.Boolean,                                SortOrder = 1 },
        new HabitEntity { Name = "Read",     Type = HabitType.Boolean,                                SortOrder = 2 },
        new HabitEntity { Name = "Water",    Type = HabitType.Numeric, Unit = "glasses", Target = 8, SortOrder = 3 },
    ];
}
