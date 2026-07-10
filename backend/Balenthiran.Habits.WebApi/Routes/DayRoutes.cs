using Balenthiran.Habits.Abstractions.Services;
using Balenthiran.Habits.WebApi.Requests;

namespace Balenthiran.Habits.WebApi.Routes;

/// <summary>
/// The daily loop (design MVP 2): fetch a day's habits with completion + streaks,
/// and log a single entry. This is the surface the Today screen drives.
/// </summary>
public static class DayRoutes
{
    public static RouteGroupBuilder MapDayRoutes(this RouteGroupBuilder parentGroup)
    {
        // A day's view: every active habit for the date with its value, completion and streak.
        parentGroup.MapGet("/days/{date}", async (DateOnly date, IDayService days) =>
                Results.Ok(await days.GetDayAsync(date)))
            .WithName("GetDay");

        // Idempotent upsert of one habit's value on one date — the core logging action.
        parentGroup.MapPut("/entries", async (EntryUpsertRequest request, IDayService days) =>
            {
                try
                {
                    var view = await days.UpsertEntryAsync(request.HabitId, request.Date, request.Value);
                    return Results.Ok(view);
                }
                catch (KeyNotFoundException)
                {
                    return Results.NotFound();
                }
            })
            .WithName("UpsertEntry");

        return parentGroup;
    }
}
