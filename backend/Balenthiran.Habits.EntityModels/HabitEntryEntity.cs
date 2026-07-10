namespace Balenthiran.Habits.EntityModels;

/// <summary>
/// One habit's log for one calendar day. At most one row per (habit, date) — logging
/// is an idempotent upsert, so re-tapping overwrites rather than appends (design A2).
/// </summary>
public class HabitEntryEntity
{
    public int Id { get; set; }

    public int HabitId { get; set; }

    /// <summary>Calendar date only, no time — the client's local "today" (design A7).</summary>
    public DateOnly Date { get; set; }

    /// <summary>1/0 for a boolean habit; the logged number for a numeric one.</summary>
    public double Value { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public HabitEntity? Habit { get; set; }
}
