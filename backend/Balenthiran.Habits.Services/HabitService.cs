using Balenthiran.Habits.Abstractions.Services;
using Balenthiran.Habits.Abstractions.Views;
using Balenthiran.Habits.Database;
using Balenthiran.Habits.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace Balenthiran.Habits.Services;

/// <summary>Manages the habit list: create, edit, reorder, archive and first-run seeding.</summary>
public class HabitService(AppDbContext db) : IHabitService
{
    public async Task<IReadOnlyList<HabitView>> GetAllAsync(bool includeArchived = false)
    {
        var query = db.Habits.AsQueryable();
        if (!includeArchived)
            query = query.Where(h => !h.IsArchived);

        var habits = await query
            .OrderBy(h => h.SortOrder)
            .ThenBy(h => h.Id)
            .ToListAsync();

        return habits.Select(ToView).ToList();
    }

    public async Task<HabitView?> GetAsync(int id)
    {
        var habit = await db.Habits.FindAsync(id);
        return habit is null ? null : ToView(habit);
    }

    public async Task<HabitView> CreateAsync(HabitInput input)
    {
        // Append to the end of the current order.
        var maxOrder = await db.Habits.AnyAsync()
            ? await db.Habits.MaxAsync(h => h.SortOrder)
            : -1;

        var habit = new HabitEntity
        {
            Name = input.Name,
            Type = input.Type,
            Unit = input.Unit,
            Target = input.Target,
            SortOrder = maxOrder + 1,
        };
        db.Habits.Add(habit);
        await db.SaveChangesAsync();
        return ToView(habit);
    }

    public async Task<HabitView?> UpdateAsync(int id, HabitInput input)
    {
        var habit = await db.Habits.FindAsync(id);
        if (habit is null)
            return null;

        habit.Name = input.Name;
        habit.Type = input.Type;
        habit.Unit = input.Unit;
        habit.Target = input.Target;
        await db.SaveChangesAsync();
        return ToView(habit);
    }

    public async Task ReorderAsync(IReadOnlyList<int> orderedHabitIds)
    {
        var habits = await db.Habits
            .Where(h => orderedHabitIds.Contains(h.Id))
            .ToDictionaryAsync(h => h.Id);

        for (var i = 0; i < orderedHabitIds.Count; i++)
        {
            if (habits.TryGetValue(orderedHabitIds[i], out var habit))
                habit.SortOrder = i;
        }
        await db.SaveChangesAsync();
    }

    public async Task<bool> ArchiveAsync(int id)
    {
        var habit = await db.Habits.FindAsync(id);
        if (habit is null)
            return false;

        habit.IsArchived = true;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<HabitView>> EnsureSeededAsync()
    {
        if (await db.Habits.AnyAsync())
            return [];

        var seeded = StarterHabits.Build();
        db.Habits.AddRange(seeded);
        await db.SaveChangesAsync();
        return seeded.Select(ToView).ToList();
    }

    private static HabitView ToView(HabitEntity h) =>
        new(h.Id, h.Name, h.Type, h.Unit, h.Target, h.SortOrder, h.IsArchived);
}
