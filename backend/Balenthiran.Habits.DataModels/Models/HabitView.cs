using Balenthiran.Habits.Abstractions.DataModels;
using Balenthiran.Habits.Abstractions.Enums;

namespace Balenthiran.Habits.DataModels.Models;

/// <inheritdoc cref="IHabitView"/>
public record HabitView(
    int Id,
    string Name,
    HabitType Type,
    string? Unit,
    double? Target,
    int SortOrder,
    bool IsArchived) : IHabitView;
