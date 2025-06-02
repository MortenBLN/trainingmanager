using Trainingsmanager.Models;

namespace Trainingsmanager.Services
{
    public interface ISubscriptionService
    {
        Task SubscribeToSessionAsync(SubscribeUsersToSessionRequest request, CancellationToken ct);
        Task DeleteSubscriptionAsync(DeleteSubscriptionRequest request, CancellationToken ct);
        Task<GetSessionResponse> GetSubscriptionsOfSessionBySessionIdAsync(Guid sessionId, CancellationToken ct);
    }
}
