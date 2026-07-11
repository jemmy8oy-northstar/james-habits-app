using Balenthiran.Habits.Abstractions.DataModels;

namespace Balenthiran.Habits.Abstractions.Services;

/// <summary>Manages the habit list itself: create, edit, reorder and archive (design MVP 1).</summary>
public interface IHabitService
{
    /// <summary>All habits in display order; archived ones excluded unless asked for.</summary>
    Task<IReadOnlyList<IHabitView>> GetAllAsync(bool includeArchived = false);

    /// <summary>A single habit by id. Throws <see cref="Exceptions.NotFoundException"/> if it does not exist.</summary>
    Task<IHabitView> GetAsync(int id);

    /// <summary>Creates a habit, appending it to the end of the current order.</summary>
    Task<IHabitView> CreateAsync(IHabitInput input);

    /// <summary>Renames / retargets a habit. Throws <see cref="Exceptions.NotFoundException"/> if it does not exist.</summary>
    Task<IHabitView> UpdateAsync(int id, IHabitInput input);

    /// <summary>Replaces the display order with the given habit ids, in sequence.</summary>
    Task ReorderAsync(IReadOnlyList<int> orderedHabitIds);

    /// <summary>Archives (soft-deletes) a habit so its history survives. Throws <see cref="Exceptions.NotFoundException"/> if missing.</summary>
    Task ArchiveAsync(int id);

    /// <summary>Seeds the starter habit set on first run; no-op once any habit exists.</summary>
    Task<IReadOnlyList<IHabitView>> EnsureSeededAsync();
}
