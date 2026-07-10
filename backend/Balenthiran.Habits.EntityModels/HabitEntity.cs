using Balenthiran.Habits.Abstractions.Enums;

namespace Balenthiran.Habits.EntityModels;

/// <summary>
/// A tracked habit. Archived rather than deleted so its entry history (and streaks)
/// survive if James stops tracking it (design A5).
/// </summary>
public class HabitEntity
{
    public int Id { get; set; }

    /// <summary>Display name, e.g. "Sleep", "Read".</summary>
    public string Name { get; set; } = string.Empty;

    public HabitType Type { get; set; }

    /// <summary>Optional unit for numeric habits, e.g. "h", "glasses". Null for boolean.</summary>
    public string? Unit { get; set; }

    /// <summary>Optional daily target for numeric habits; completion is value ≥ target (A3).</summary>
    public double? Target { get; set; }

    /// <summary>Position in the Today / Manage lists; lower shows first.</summary>
    public int SortOrder { get; set; }

    public bool IsArchived { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<HabitEntryEntity> Entries { get; set; } = new List<HabitEntryEntity>();
}
