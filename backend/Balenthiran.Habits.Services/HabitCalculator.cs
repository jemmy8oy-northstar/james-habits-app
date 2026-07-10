using Balenthiran.Habits.Abstractions.Enums;
using Balenthiran.Habits.Abstractions.Services;

namespace Balenthiran.Habits.Services;

/// <inheritdoc cref="IHabitCalculator"/>
public class HabitCalculator : IHabitCalculator
{
    public bool IsComplete(HabitType type, double? target, double? value)
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

    public int CurrentStreak(IReadOnlySet<DateOnly> completeDates, DateOnly asOf)
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

    public int LongestStreak(IEnumerable<DateOnly> completeDates)
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
