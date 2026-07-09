using Balenthiran.Habits.Abstractions.DataModels;
using Balenthiran.Habits.Abstractions.DomainModels;
using Balenthiran.Habits.Abstractions.Services;

namespace Balenthiran.Habits.WebApi.Routes;

public static class StatusRoutes
{
    public static RouteGroupBuilder MapStatusRoutes(this RouteGroupBuilder parentGroup)
    {
        var group = parentGroup.MapGroup("/status");

        group.MapGet("", async (IStatusService statusService) =>
        {
            var status = await statusService.GetSystemStatusAsync();
            return Results.Ok(new {
                version = status.Version,
                friendlyStatus = status.GetFriendlyStatus(),
                timestamp = status.LastUpdated
            });
        })
        .WithName("GetStatus");

        return parentGroup;
    }
}
