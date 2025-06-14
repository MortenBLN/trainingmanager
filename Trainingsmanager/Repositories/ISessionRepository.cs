using Trainingsmanager.Database.Models;

namespace Trainingsmanager.Repositories
{
    public interface ISessionRepository
    {
        Task<Session> CreateSessionAsync(Session request, CancellationToken ct);
        Task<Session> GetSessionByIdAsync(Guid sessionId, CancellationToken ct);
        Task<Session> GetSessionBySubscriptionIdAsync(Guid subscriptionId, CancellationToken ct);
        Task<List<Session>> GetAllSessions(CancellationToken ct);
        Task DeleteSessionAsync(Guid sessionId, CancellationToken ct);
        Task<Session?> UpdateSessionAsync(Session sessionToUpdateWithNewValues, CancellationToken ct);
    }
}
