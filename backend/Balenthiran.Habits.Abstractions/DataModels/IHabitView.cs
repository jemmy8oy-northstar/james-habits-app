using Balenthiran.Habits.Abstractions.Enums;

namespace Balenthiran.Habits.Abstractions.DataModels;

/// <summary>
/// A habit as shown on the Manage screen or returned from CRUD. A plain projection
/// of the stored habit — the persistence entity never crosses the service boundary.
/// </summary>
public interface IHabitView
{
    int Id { get; }
    string Name { get; }
    HabitType Type { get; }
    string? Unit { get; }
    double? Target { get; }
    int SortOrder { get; }
    bool IsArchived { get; }
}
