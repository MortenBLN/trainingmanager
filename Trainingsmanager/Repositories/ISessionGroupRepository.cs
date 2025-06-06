using Trainingsmanager.Database.Models;

namespace Trainingsmanager.Repositories
{
    public interface ISessionGroupRepository
    {
        Task<SessionGroup?> CreateSessionGroupAsync(List<Session> sessions, CancellationToken ct);
        Task DeleteSessionAsync(Guid groupId, CancellationToken ct);
    }
}
