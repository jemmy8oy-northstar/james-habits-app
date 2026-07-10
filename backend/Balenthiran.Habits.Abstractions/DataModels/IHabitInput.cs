using Balenthiran.Habits.Abstractions.Enums;

namespace Balenthiran.Habits.Abstractions.DataModels;

/// <summary>The fields a caller supplies to create or edit a habit.</summary>
public interface IHabitInput
{
    string Name { get; }
    HabitType Type { get; }
    string? Unit { get; }
    double? Target { get; }
}
