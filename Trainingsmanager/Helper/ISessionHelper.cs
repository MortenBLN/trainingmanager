using Trainingsmanager.Models;

namespace Trainingsmanager.Helper
{
    public interface ISessionHelper
    {
        CreateSessionRequest AddWeeksToDates(CreateSessionRequest request, int weeksToAdd, CancellationToken ct);
    }
}
