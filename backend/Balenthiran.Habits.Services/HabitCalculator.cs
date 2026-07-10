using Balenthiran.Habits.Abstractions.Enums;

namespace Balenthiran.Habits.Services;

/// <summary>
/// The pure completion + streak rules (design A3/A4). No database, no clock —
/// every input is passed in, so the app's momentum logic is tested directly and
/// exhaustively (mirrors language-vocab's PoolMath).
/// </summary>
public static class HabitCalculator
{
    /// <summary>
    /// Whether a logged value completes the day for a habit (A3):
    /// boolean → ticked (non-zero); numeric with a target → value ≥ target;
    /// numeric without a target → any value logged. A missing value is never complete.
    /// </summary>
    public static bool IsComplete(HabitType type, double? target, double? value)
    {
        if (value is null)
            return false;

        return type switch
        {
            HabitType.Boolean => value.Value != 0d,
            HabitType.Numeric => !target.HasValue || value.Value >= target.Value,
            _ => false,
        };
    }

    /// <summary>
    /// Consecutive complete days counting back from <paramref name="asOf"/> (A4).
    /// The as-of day itself is a grace day: not yet completing today doesn't break a
    /// streak until the day ends, so if it isn't complete we start counting from the day before.
    /// </summary>
    public static int CurrentStreak(IReadOnlySet<DateOnly> completeDates, DateOnly asOf)
    {
        var cursor = completeDates.Contains(asOf) ? asOf : asOf.AddDays(-1);
        var streak = 0;
        while (completeDates.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }
        return streak;
    }

    /// <summary>The longest run of consecutive complete calendar days ever recorded.</summary>
    public static int LongestStreak(IEnumerable<DateOnly> completeDates)
    {
        var sorted = completeDates.Distinct().OrderBy(d => d).ToList();
        int longest = 0, run = 0;
        DateOnly? prev = null;
        foreach (var d in sorted)
        {
            run = prev is DateOnly p && d == p.AddDays(1) ? run + 1 : 1;
            if (run > longest)
                longest = run;
            prev = d;
        }
        return longest;
    }
}
