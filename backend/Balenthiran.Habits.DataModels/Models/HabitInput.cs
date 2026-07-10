using Balenthiran.Habits.Abstractions.DataModels;
using Balenthiran.Habits.Abstractions.Enums;

namespace Balenthiran.Habits.DataModels.Models;

/// <inheritdoc cref="IHabitInput"/>
public record HabitInput(
    string Name,
    HabitType Type,
    string? Unit = null,
    double? Target = null) : IHabitInput;
