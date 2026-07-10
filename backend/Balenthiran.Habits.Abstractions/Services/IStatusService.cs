using Balenthiran.Habits.Abstractions.DataModels;
using Balenthiran.Habits.Abstractions.DomainModels;

namespace Balenthiran.Habits.Abstractions.Services;

public interface IStatusService
{
    Task<IDomainStatus> GetSystemStatusAsync();
}
