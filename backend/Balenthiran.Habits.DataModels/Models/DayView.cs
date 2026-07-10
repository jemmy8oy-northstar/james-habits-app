using Balenthiran.Habits.Abstractions.DataModels;

namespace Balenthiran.Habits.DataModels.Models;

/// <inheritdoc cref="IDayView"/>
public record DayView(
    DateOnly Date,
    IReadOnlyList<IHabitDayView> Habits) : IDayView;
