using Trainingsmanager.Database.Models;
using Trainingsmanager.Models.DTOs;

namespace Trainingsmanager.Repositories
{
    public interface ISessionRepository
    {
        Task<Session> CreateSession(Session request, CancellationToken ct);
        Task<Session> GetSessionByIdAsync(Guid sessionId, CancellationToken ct);
        Task<List<Session>> GetAllSessions(CancellationToken ct);
    }
}
