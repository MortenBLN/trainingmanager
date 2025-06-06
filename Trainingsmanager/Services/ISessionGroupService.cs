using Trainingsmanager.Models;

namespace Trainingsmanager.Services
{
    public interface ISessionGroupService
    {
        Task DeleteSessionGroupAsync(DeleteSessionGroupRequest req, CancellationToken ct);
    }
}
