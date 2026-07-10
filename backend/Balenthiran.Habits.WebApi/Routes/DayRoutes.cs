using AutoMapper;
using Balenthiran.Habits.Abstractions.Services;
using Balenthiran.Habits.DataModels.Models;
using Balenthiran.Habits.WebApi.Requests;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Balenthiran.Habits.WebApi.Routes;

/// <summary>
/// The daily loop (design MVP 2): fetch a day's habits with completion + streaks,
/// and log a single entry. This is the surface the Today screen drives. Handlers are
/// named delegates with concrete <c>TypedResults</c> return types so OpenAPI can
/// describe each response body; service views are mapped to records via <see cref="IMapper"/>.
/// </summary>
public static class DayRoutes
{
    public static RouteGroupBuilder MapDayRoutes(this RouteGroupBuilder parentGroup)
    {
        parentGroup.MapGet("/days/{date}", GetDay).WithName("GetDay");
        parentGroup.MapPut("/entries", UpsertEntry).WithName("UpsertEntry");

        return parentGroup;
    }

    // A day's view: every active habit for the date with its value, completion and streak.
    private static async Task<Ok<DayView>> GetDay(DateOnly date, IDayService days, IMapper mapper) =>
        TypedResults.Ok(mapper.Map<DayView>(await days.GetDayAsync(date)));

    // Idempotent upsert of one habit's value on one date — the core logging action.
    private static async Task<Results<Ok<HabitDayView>, NotFound>> UpsertEntry(
        EntryUpsertRequest request, IDayService days, IMapper mapper)
    {
        try
        {
            var view = await days.UpsertEntryAsync(request.HabitId, request.Date, request.Value);
            return TypedResults.Ok(mapper.Map<HabitDayView>(view));
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}
