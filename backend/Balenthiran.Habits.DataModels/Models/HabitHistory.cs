using Balenthiran.Habits.Abstractions.DataModels;

namespace Balenthiran.Habits.DataModels.Models;

/// <inheritdoc cref="IHabitHistory"/>
public record HabitHistory(
    int HabitId,
    string Name,
    int CurrentStreak,
    int LongestStreak,
    IReadOnlyList<DayCompletion> Days) : IHabitHistory
{
    // Concrete list on the record for a concrete OpenAPI item schema; the explicit
    // member keeps the interface's collection-of-interface contract intact.
    IReadOnlyList<IDayCompletion> IHabitHistory.Days => Days;
}
