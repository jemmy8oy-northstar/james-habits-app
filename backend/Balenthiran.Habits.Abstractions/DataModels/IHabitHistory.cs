namespace Balenthiran.Habits.Abstractions.DataModels;

/// <summary>Per-habit history: current/longest streak and a completion grid ending today.</summary>
public interface IHabitHistory
{
    int HabitId { get; }
    string Name { get; }
    int CurrentStreak { get; }
    int LongestStreak { get; }
    IReadOnlyList<IDayCompletion> Days { get; }
}
