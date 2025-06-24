using Trainingsmanager.Database.Models;
using Trainingsmanager.Models;

namespace Trainingsmanager.Services
{
    public interface ISubscriptionService
    {
        Task<SubscribeUserToSessionResponse> SubscribeToSessionAsync(SubscribeUserToSessionRequest request, CancellationToken ct);
        Task DeleteSubscriptionAsync(DeleteSubscriptionRequest request, CancellationToken ct);
        Task<GetSessionResponse> GetSubscriptionsOfSessionBySessionIdAsync(Guid sessionId, CancellationToken ct);
        Task UpgradeSubscription(Subscription subscription, Session session, CancellationToken ct);
    }
}
