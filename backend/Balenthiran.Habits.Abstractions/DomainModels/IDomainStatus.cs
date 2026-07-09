namespace Balenthiran.Habits.Abstractions.DomainModels;

using Balenthiran.Habits.Abstractions.DataModels;

public interface IDomainStatus : IStatus
{
    string GetFriendlyStatus();
}
