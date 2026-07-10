namespace Balenthiran.Habits.Abstractions.DataModels;

/// <summary>Whether a single day counted as complete, and the value logged (if any).</summary>
public interface IDayCompletion
{
    DateOnly Date { get; }
    bool IsComplete { get; }
    double? Value { get; }
}
