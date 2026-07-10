using AutoMapper;
using Balenthiran.Habits.Abstractions.DataModels;
using Balenthiran.Habits.DataModels.Models;

namespace Balenthiran.Habits.WebApi;

/// <summary>
/// API-boundary maps: the service layer's view interfaces → the concrete response
/// records the API puts on the wire. Route handlers declare concrete return types
/// (e.g. <c>Ok&lt;HabitView&gt;</c>) so the OpenAPI document can describe every response
/// body — which is what the frontend codegen turns into typed RTK-Query hooks.
/// Services still deal in interfaces; this profile is the one place the wire contract
/// is pinned to a concrete shape. Discovered by <c>AddAutoMapper(AddMaps(...))</c>.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<IHabitView, HabitView>();
        CreateMap<IHabitDayView, HabitDayView>();
        CreateMap<IDayCompletion, DayCompletion>();
        CreateMap<IDayView, DayView>();
        CreateMap<IHabitHistory, HabitHistory>();
    }
}
