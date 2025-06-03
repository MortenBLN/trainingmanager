using Trainingsmanager.Models;

namespace Trainingsmanager.Services
{
    public interface ISessionService
    {
        Task<CreateSessionsResponse> CreateSession(CreateSessionRequest request, CancellationToken ct);
        Task<GetSessionResponse> GetSessionById(Guid sessionId, CancellationToken ct);
        Task<GetAllSessionsResponse> GetAllSessions(CancellationToken ct);
        Task DeleteSessionAsync(DeleteSessionRequest req, CancellationToken ct);
    }
}
