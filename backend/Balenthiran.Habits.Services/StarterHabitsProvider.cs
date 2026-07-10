using Balenthiran.Habits.Abstractions.DataModels;
using Balenthiran.Habits.Abstractions.Enums;
using Balenthiran.Habits.Abstractions.Services;
using Balenthiran.Habits.DataModels.Models;

namespace Balenthiran.Habits.Services;

/// <inheritdoc cref="IStarterHabitsProvider"/>
public class StarterHabitsProvider : IStarterHabitsProvider
{
    public IReadOnlyList<IHabitInput> GetStarterHabits() =>
    [
        new HabitInput("Sleep", HabitType.Numeric, "h", 8),
        new HabitInput("Exercise", HabitType.Boolean),
        new HabitInput("Read", HabitType.Boolean),
        new HabitInput("Water", HabitType.Numeric, "glasses", 8),
    ];
}
