using Balenthiran.Habits.Abstractions.Views;

namespace Balenthiran.Habits.Abstractions.Services;

/// <summary>Manages the habit list itself: create, edit, reorder and archive (design MVP 1).</summary>
public interface IHabitService
{
    /// <summary>All habits in display order; archived ones excluded unless asked for.</summary>
    Task<IReadOnlyList<HabitView>> GetAllAsync(bool includeArchived = false);

    Task<HabitView?> GetAsync(int id);

    /// <summary>Creates a habit, appending it to the end of the current order.</summary>
    Task<HabitView> CreateAsync(HabitInput input);

    /// <summary>Renames / retargets a habit. Returns null if it does not exist.</summary>
    Task<HabitView?> UpdateAsync(int id, HabitInput input);

    /// <summary>Replaces the display order with the given habit ids, in sequence.</summary>
    Task ReorderAsync(IReadOnlyList<int> orderedHabitIds);

    /// <summary>Archives (soft-deletes) a habit so its history survives. Returns false if missing.</summary>
    Task<bool> ArchiveAsync(int id);

    /// <summary>Seeds the starter habit set on first run; no-op once any habit exists.</summary>
    Task<IReadOnlyList<HabitView>> EnsureSeededAsync();
}
