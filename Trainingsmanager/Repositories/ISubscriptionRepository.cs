using Trainingsmanager.Database.Enums;
using Trainingsmanager.Database.Models;
using Trainingsmanager.Models;

namespace Trainingsmanager.Repositories
{
    public interface ISubscriptionRepository
    {
        Task<Subscription> SubscribeToSessionAsync(Subscription subscription, CancellationToken ct);
        Task<Subscription> DeleteSubscriptionAsync(DeleteSubscriptionRequest request, CancellationToken ct);
        Task UpgradeSubscriptionTypeAsync(Subscription subscriptionToUpgrade, SubscriptionType newSubscriptionType, CancellationToken ct);
        Task<List<Subscription>> GetSubscriptionsOfSessionBySessionIdAsync(Guid sessionId, CancellationToken ct);
    }
}
