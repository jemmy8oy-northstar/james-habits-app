namespace Balenthiran.Habits.Abstractions.DataModels;

/// <summary>The daily-loop payload: every active habit for a date, in display order.</summary>
public interface IDayView
{
    DateOnly Date { get; }
    IReadOnlyList<IHabitDayView> Habits { get; }
}
