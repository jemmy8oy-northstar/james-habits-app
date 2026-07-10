using Balenthiran.Habits.Abstractions.DataModels;

namespace Balenthiran.Habits.DataModels.Models;

/// <inheritdoc cref="IHabitHistory"/>
public record HabitHistory(
    int HabitId,
    string Name,
    int CurrentStreak,
    int LongestStreak,
    IReadOnlyList<IDayCompletion> Days) : IHabitHistory;
