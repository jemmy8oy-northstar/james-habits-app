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
        // Missing-resource routes throw NotFoundException in the service; the global handler
        // (issue #7) turns that into a 404 ProblemDetails. ProducesProblem keeps the OpenAPI
        // document honest so the generated client knows a 404 is possible.
        group.MapGet("/{id:int}", GetHabit).WithName("GetHabit").ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPost("", CreateHabit).WithName("CreateHabit");
        group.MapPut("/{id:int}", UpdateHabit).WithName("UpdateHabit").ProducesProblem(StatusCodes.Status404NotFound);
        // Reorder before "/{id}" DELETE so "/reorder" is never captured as an id.
        group.MapPut("/reorder", ReorderHabits).WithName("ReorderHabits");
        // Archive is a soft delete: the habit's history survives (design MVP 1).
        group.MapDelete("/{id:int}", ArchiveHabit).WithName("ArchiveHabit").ProducesProblem(StatusCodes.Status404NotFound);
        group.MapGet("/{id:int}/history", GetHabitHistory).WithName("GetHabitHistory").ProducesProblem(StatusCodes.Status404NotFound);

        return parentGroup;
    }

    private static async Task<Ok<IReadOnlyList<HabitView>>> GetHabits(
        IHabitService habits, IMapper mapper, bool includeArchived = false) =>
        TypedResults.Ok(mapper.Map<IReadOnlyList<HabitView>>(await habits.GetAllAsync(includeArchived)));

    private static async Task<Ok<HabitView>> GetHabit(int id, IHabitService habits, IMapper mapper) =>
        TypedResults.Ok(mapper.Map<HabitView>(await habits.GetAsync(id)));

    private static async Task<Created<HabitView>> CreateHabit(
        HabitInput input, IHabitService habits, IMapper mapper)
    {
        var created = await habits.CreateAsync(input);
        return TypedResults.Created($"/api/habits/{created.Id}", mapper.Map<HabitView>(created));
    }

    private static async Task<Ok<HabitView>> UpdateHabit(
        int id, HabitInput input, IHabitService habits, IMapper mapper) =>
        TypedResults.Ok(mapper.Map<HabitView>(await habits.UpdateAsync(id, input)));

    private static async Task<NoContent> ReorderHabits(
        IReadOnlyList<int> orderedHabitIds, IHabitService habits)
    {
        await habits.ReorderAsync(orderedHabitIds);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> ArchiveHabit(int id, IHabitService habits)
    {
        await habits.ArchiveAsync(id);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<HabitHistory>> GetHabitHistory(
        int id, IDayService days, IMapper mapper, int historyDays = 30, DateOnly? today = null)
    {
        var anchor = today ?? DateOnly.FromDateTime(DateTime.Today);
        var history = await days.GetHistoryAsync(id, anchor, historyDays);
        return TypedResults.Ok(mapper.Map<HabitHistory>(history));
    }
}
