using Balenthiran.Habits.Abstractions.Enums;

namespace Balenthiran.Habits.Abstractions.DataModels;

/// <summary>
/// One habit's slot within a single day's view: its current entry value (if any),
/// whether that makes the day complete, and the streak as of that date.
/// </summary>
public interface IHabitDayView
{
    int HabitId { get; }
    string Name { get; }
    HabitType Type { get; }
    string? Unit { get; }
    double? Target { get; }
    int SortOrder { get; }
    double? Value { get; }
    bool IsComplete { get; }
    int CurrentStreak { get; }
}
