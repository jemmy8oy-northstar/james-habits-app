using Balenthiran.Habits.Abstractions.Enums;

namespace Balenthiran.Habits.Abstractions.Views;

/// <summary>
/// A habit as shown on the Manage screen or returned from CRUD. A plain projection
/// of the stored habit — the persistence entity never crosses the service boundary.
/// </summary>
public record HabitView(
    int Id,
    string Name,
    HabitType Type,
    string? Unit,
    double? Target,
    int SortOrder,
    bool IsArchived);

/// <summary>The fields a caller supplies to create or edit a habit.</summary>
public record HabitInput(
    string Name,
    HabitType Type,
    string? Unit = null,
    double? Target = null);

/// <summary>
/// One habit's slot within a single day's view: its current entry value (if any),
/// whether that makes the day complete, and the streak as of that date.
/// </summary>
public record HabitDayView(
    int HabitId,
    string Name,
    HabitType Type,
    string? Unit,
    double? Target,
    int SortOrder,
    double? Value,
    bool IsComplete,
    int CurrentStreak);

/// <summary>The daily-loop payload: every active habit for a date, in display order.</summary>
public record DayView(
    DateOnly Date,
    IReadOnlyList<HabitDayView> Habits);

/// <summary>Whether a single day counted as complete, and the value logged (if any).</summary>
public record DayCompletion(
    DateOnly Date,
    bool IsComplete,
    double? Value);

/// <summary>Per-habit history: current/longest streak and a completion grid ending today.</summary>
public record HabitHistory(
    int HabitId,
    string Name,
    int CurrentStreak,
    int LongestStreak,
    IReadOnlyList<DayCompletion> Days);
