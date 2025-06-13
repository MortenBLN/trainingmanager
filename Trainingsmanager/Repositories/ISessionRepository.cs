using Trainingsmanager.Database.Models;
using Trainingsmanager.Models;

namespace Trainingsmanager.Repositories
{
    public interface ISessionRepository
    {
        Task<Session> CreateSessionAsync(Session request, CancellationToken ct);
        Task<Session> GetSessionByIdAsync(Guid sessionId, CancellationToken ct);
        Task<List<Session>> GetAllSessions(CancellationToken ct);
        Task DeleteSessionAsync(Guid sessionId, CancellationToken ct);
        Task<Session?> UpdateSessionAsync(Session sessionToUpdateWithNewValues, CancellationToken ct);
    }
}
