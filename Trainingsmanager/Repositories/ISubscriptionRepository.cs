using Trainingsmanager.Database.Enums;
using Trainingsmanager.Database.Models;
using Trainingsmanager.Models;

namespace Trainingsmanager.Repositories
{
    public interface ISubscriptionRepository
    {
        Task SubscribeToSessionAsync(SubscribeUsersToSessionRequest request, CancellationToken ct, SubscriptionType subType = SubscriptionType.Gast);
        Task DeleteSubscriptionAsync(DeleteSubscriptionRequest request, CancellationToken ct);
        Task<List<Subscription>> GetSubscriptionsOfSessionBySessionIdAsync(Guid sessionId, CancellationToken ct);
    }
}
