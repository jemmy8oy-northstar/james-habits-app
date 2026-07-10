using Balenthiran.Habits.Abstractions.DomainModels;
using Balenthiran.Habits.DataModels.Models;

namespace Balenthiran.Habits.DomainModels.Models;

public class DomainStatus : Status, IDomainStatus
{
    public string GetFriendlyStatus()
    {
        return $"System is running version {Version} (Updated: {LastUpdated:g})";
    }
}
