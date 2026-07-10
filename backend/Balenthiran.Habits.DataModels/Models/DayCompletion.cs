using Balenthiran.Habits.Abstractions.DataModels;

namespace Balenthiran.Habits.DataModels.Models;

/// <inheritdoc cref="IDayCompletion"/>
public record DayCompletion(
    DateOnly Date,
    bool IsComplete,
    double? Value) : IDayCompletion;
