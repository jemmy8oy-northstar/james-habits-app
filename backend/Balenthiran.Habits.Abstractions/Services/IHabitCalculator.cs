using Balenthiran.Habits.Abstractions.Enums;

namespace Balenthiran.Habits.Abstractions.Services;

/// <summary>
/// The pure completion + streak rules (design A3/A4). No database, no clock —
/// every input is passed in, so the app's momentum logic is tested directly and
/// exhaustively. A DI service so it can be injected, swapped or logged.
/// </summary>
public interface IHabitCalculator
{
    /// <summary>
    /// Whether a logged value completes the day for a habit (A3):
    /// boolean → ticked (non-zero); numeric with a target → value ≥ target;
    /// numeric without a target → any value logged. A missing value is never complete.
    /// </summary>
    bool IsComplete(HabitType type, double? target, double? value);

    /// <summary>
    /// Consecutive complete days counting back from <paramref name="asOf"/> (A4).
    /// The as-of day itself is a grace day: not yet completing today doesn't break a
    /// streak until the day ends.
    /// </summary>
    int CurrentStreak(IReadOnlySet<DateOnly> completeDates, DateOnly asOf);

    /// <summary>The longest run of consecutive complete calendar days ever recorded.</summary>
    int LongestStreak(IEnumerable<DateOnly> completeDates);
}
