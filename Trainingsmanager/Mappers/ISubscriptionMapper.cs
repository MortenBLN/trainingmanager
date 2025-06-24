using Trainingsmanager.Database.Enums;
using Trainingsmanager.Database.Models;
using Trainingsmanager.Models;
using Trainingsmanager.Models.Enums;

namespace Trainingsmanager.Mappers
{
    public interface ISubscriptionMapper
    {
        SubscribeUserToSessionResponse SubscriptionToSubscribeUsersToSessionResponse(Subscription subscription, CancellationToken ct);
        Subscription SubscribeUserToSessionRequestToSession(SubscribeUserToSessionRequest request, SubscriptionTypeDto subscriptionType, CancellationToken ct);
        Subscription SubscriptionToUpgradedSubscription(Subscription oldestQueuedSubscription, SubscriptionType newSubscriptionType);
    }
}
