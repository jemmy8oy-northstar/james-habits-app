using Balenthiran.Habits.Abstractions.Services;
using Balenthiran.Habits.DataModels.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Balenthiran.Habits.WebApi.Routes;

/// <summary>
/// The health/status endpoint. A named delegate returning a concrete
/// <see cref="StatusResponse"/> so OpenAPI describes the payload (previously an
/// anonymous object, which produced an untyped response in the generated client).
/// The route assembles the response shape, so it is constructed here directly.
/// </summary>
public static class StatusRoutes
{
    public static RouteGroupBuilder MapStatusRoutes(this RouteGroupBuilder parentGroup)
    {
        var group = parentGroup.MapGroup("/status");

        group.MapGet("", GetStatus).WithName("GetStatus");

        return parentGroup;
    }

    private static async Task<Ok<StatusResponse>> GetStatus(IStatusService statusService)
    {
        var status = await statusService.GetSystemStatusAsync();
        return TypedResults.Ok(new StatusResponse(
            status.Version,
            status.GetFriendlyStatus(),
            status.LastUpdated));
    }
}
