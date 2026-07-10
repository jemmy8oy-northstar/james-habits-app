namespace Balenthiran.Habits.Abstractions.Enums;

/// <summary>
/// The kind of habit, which decides how a day's entry is captured and when it
/// counts as "complete" (design A1/A3).
/// </summary>
public enum HabitType
{
    /// <summary>Did / didn't — a checkbox habit (Ran, Meditated). Entry value 1 = done, 0 = not.</summary>
    Boolean = 0,

    /// <summary>A number with an optional unit and optional daily target (Sleep h, Water glasses).</summary>
    Numeric = 1,
}
