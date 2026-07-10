using Balenthiran.Habits.Abstractions.DataModels;
using Balenthiran.Habits.Abstractions.Services;
using Balenthiran.Habits.DataModels.Models;

namespace Balenthiran.Habits.WebApi.Routes;

/// <summary>
/// The habit list itself (design MVP 1): CRUD, reorder, archive, plus a habit's
/// history grid. Handlers return the service's view interfaces directly — the
/// persistence entity never reaches the wire.
/// </summary>
public static class HabitRoutes
{
    public static RouteGroupBuilder MapHabitRoutes(this RouteGroupBuilder parentGroup)
    {
        var group = parentGroup.MapGroup("/habits");

        group.MapGet("", async (IHabitService habits, bool includeArchived = false) =>
                Results.Ok(await habits.GetAllAsync(includeArchived)))
            .WithName("GetHabits");

        group.MapGet("/{id:int}", async (int id, IHabitService habits) =>
                await habits.GetAsync(id) is { } habit
                    ? Results.Ok(habit)
                    : Results.NotFound())
            .WithName("GetHabit");

        group.MapPost("", async (HabitInput input, IHabitService habits) =>
            {
                var created = await habits.CreateAsync(input);
                return Results.Created($"/api/habits/{created.Id}", created);
            })
            .WithName("CreateHabit");

        group.MapPut("/{id:int}", async (int id, HabitInput input, IHabitService habits) =>
                await habits.UpdateAsync(id, input) is { } updated
                    ? Results.Ok(updated)
                    : Results.NotFound())
            .WithName("UpdateHabit");

        // Reorder before "/{id}" DELETE so "/reorder" is never captured as an id.
        group.MapPut("/reorder", async (IReadOnlyList<int> orderedHabitIds, IHabitService habits) =>
            {
                await habits.ReorderAsync(orderedHabitIds);
                return Results.NoContent();
            })
            .WithName("ReorderHabits");

        // Archive is a soft delete: the habit's history survives (design MVP 1).
        group.MapDelete("/{id:int}", async (int id, IHabitService habits) =>
                await habits.ArchiveAsync(id)
                    ? Results.NoContent()
                    : Results.NotFound())
            .WithName("ArchiveHabit");

        group.MapGet("/{id:int}/history", async (int id, IDayService days, int historyDays = 30, DateOnly? today = null) =>
            {
                var anchor = today ?? DateOnly.FromDateTime(DateTime.Today);
                return await days.GetHistoryAsync(id, anchor, historyDays) is { } history
                    ? Results.Ok(history)
                    : Results.NotFound();
            })
            .WithName("GetHabitHistory");

        return parentGroup;
    }
}
