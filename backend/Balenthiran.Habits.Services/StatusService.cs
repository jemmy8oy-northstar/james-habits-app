using Balenthiran.Habits.Abstractions.DataModels;
using Balenthiran.Habits.Abstractions.DomainModels;
using Balenthiran.Habits.Abstractions.Services;
using Balenthiran.Habits.DomainModels.Models;

namespace Balenthiran.Habits.Services;

public class StatusService : IStatusService
{
    public Task<IDomainStatus> GetSystemStatusAsync()
    {
        IDomainStatus model = new DomainStatus
        {
            Version = "1.1.0-alpha",
            LastUpdated = DateTime.UtcNow
        };

        return Task.FromResult(model);
    }
}
