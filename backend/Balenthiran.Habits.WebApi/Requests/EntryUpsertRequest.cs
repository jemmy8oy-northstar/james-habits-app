namespace Balenthiran.Habits.WebApi.Requests;

/// <summary>
/// Body for logging one habit's value on one date. Value is 1/0 for boolean
/// habits and the measured amount for numeric ones (design MVP 2).
/// </summary>
public record EntryUpsertRequest(int HabitId, DateOnly Date, double Value);
