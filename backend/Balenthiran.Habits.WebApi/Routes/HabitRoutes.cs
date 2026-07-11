using AutoMapper;
using Balenthiran.Habits.Abstractions.Services;
using Balenthiran.Habits.DataModels.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Balenthiran.Habits.WebApi.Routes;

/// <summary>
/// The habit list itself (design MVP 1): CRUD, reorder, archive, plus a habit's
/// history grid. Handlers are named delegates with concrete <c>TypedResults</c>
/// return types so the OpenAPI document describes each response body; the service's
/// view interfaces are mapped to concrete response records via <see cref="IMapper"/>.
/// </summary>
public static class HabitRoutes
{
    public static RouteGroupBuilder MapHabitRoutes(this RouteGroupBuilder parentGroup)
    {
        var group = parentGroup.MapGroup("/habits");

        group.MapGet("", GetHabits).WithName("GetHabits");
        group.MapGet("/{id:int}", GetHabit).WithName("GetHabit");
        group.MapPost("", CreateHabit).WithName("CreateHabit");
        group.MapPut("/{id:int}", UpdateHabit).WithName("UpdateHabit");
        // Reorder before "/{id}" DELETE so "/reorder" is never captured as an id.
        group.MapPut("/reorder", ReorderHabits).WithName("ReorderHabits");
        // Archive is a soft delete: the habit's history survives (design MVP 1).
        group.MapDelete("/{id:int}", ArchiveHabit).WithName("ArchiveHabit");
        group.MapGet("/{id:int}/history", GetHabitHistory).WithName("GetHabitHistory");

        return parentGroup;
    }

    private static async Task<Ok<IReadOnlyList<HabitView>>> GetHabits(
        IHabitService habits, IMapper mapper, bool includeArchived = false) =>
        TypedResults.Ok(mapper.Map<IReadOnlyList<HabitView>>(await habits.GetAllAsync(includeArchived)));

    private static async Task<Results<Ok<HabitView>, NotFound>> GetHabit(
        int id, IHabitService habits, IMapper mapper) =>
        await habits.GetAsync(id) is { } habit
            ? TypedResults.Ok(mapper.Map<HabitView>(habit))
            : TypedResults.NotFound();

    private static async Task<Created<HabitView>> CreateHabit(
        HabitInput input, IHabitService habits, IMapper mapper)
    {
        var created = await habits.CreateAsync(input);
        return TypedResults.Created($"/api/habits/{created.Id}", mapper.Map<HabitView>(created));
    }

    private static async Task<Results<Ok<HabitView>, NotFound>> UpdateHabit(
        int id, HabitInput input, IHabitService habits, IMapper mapper) =>
        await habits.UpdateAsync(id, input) is { } updated
            ? TypedResults.Ok(mapper.Map<HabitView>(updated))
            : TypedResults.NotFound();

    private static async Task<NoContent> ReorderHabits(
        IReadOnlyList<int> orderedHabitIds, IHabitService habits)
    {
        await habits.ReorderAsync(orderedHabitIds);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, NotFound>> ArchiveHabit(int id, IHabitService habits) =>
        await habits.ArchiveAsync(id)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();

    private static async Task<Results<Ok<HabitHistory>, NotFound>> GetHabitHistory(
        int id, IDayService days, IMapper mapper, int historyDays = 30, DateOnly? today = null)
    {
        var anchor = today ?? DateOnly.FromDateTime(DateTime.Today);
        return await days.GetHistoryAsync(id, anchor, historyDays) is { } history
            ? TypedResults.Ok(mapper.Map<HabitHistory>(history))
            : TypedResults.NotFound();
    }
}
