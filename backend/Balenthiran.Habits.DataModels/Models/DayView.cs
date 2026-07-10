using Balenthiran.Habits.Abstractions.DataModels;

namespace Balenthiran.Habits.DataModels.Models;

/// <inheritdoc cref="IDayView"/>
public record DayView(
    DateOnly Date,
    IReadOnlyList<HabitDayView> Habits) : IDayView
{
    // Expose the concrete list on the record (so OpenAPI infers a concrete item
    // schema) while still satisfying the interface's collection-of-interface shape.
    IReadOnlyList<IHabitDayView> IDayView.Habits => Habits;
}
