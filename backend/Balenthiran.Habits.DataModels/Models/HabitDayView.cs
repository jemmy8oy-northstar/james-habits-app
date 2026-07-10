using Balenthiran.Habits.Abstractions.DataModels;
using Balenthiran.Habits.Abstractions.Enums;

namespace Balenthiran.Habits.DataModels.Models;

/// <inheritdoc cref="IHabitDayView"/>
public record HabitDayView(
    int HabitId,
    string Name,
    HabitType Type,
    string? Unit,
    double? Target,
    int SortOrder,
    double? Value,
    bool IsComplete,
    int CurrentStreak) : IHabitDayView;
