using Trainingsmanager.Models;

namespace Trainingsmanager.Services
{
    public interface ISessionService
    {
        Task<CreateSessionsResponse> CreateSessionAsync(CreateSessionRequest request, CancellationToken ct);
        Task<GetSessionResponse> GetSessionByIdAsync(Guid sessionId, CancellationToken ct);
        Task<GetAllSessionsResponse> GetAllSessionsAsync(CancellationToken ct);
        Task DeleteSessionAsync(DeleteSessionRequest req, CancellationToken ct);
        Task<GetSessionResponse?> UpdateSessionAsync(UpdateSessionRequest req, CancellationToken ct);
    }
}
