using Trainingsmanager.Database.Enums;
using Trainingsmanager.Database.Models;
using Trainingsmanager.Models;
using Trainingsmanager.Models.Enums;

namespace Trainingsmanager.Mappers
{
    public class SubscriptionMapper : ISubscriptionMapper
    {
        public SubscribeUserToSessionResponse SubscriptionToSubscribeUsersToSessionResponse(Subscription subscription, CancellationToken ct)
        {
            return new SubscribeUserToSessionResponse
            {
                SessionId = subscription.SessionId,
                Name = subscription.UserName,
                SubscriptionType = SubscriptionTypeToSubscriptionTypeDto(subscription.SubscriptionType),
            };
        }

        public Subscription SubscribeUserToSessionRequestToSession(SubscribeUserToSessionRequest request, SubscriptionTypeDto subscriptionType, CancellationToken ct)
        {
            if (request.Name == null)
            {
                throw new ArgumentNullException("Name cannot be null");
            }

            return new Subscription
            {
                SessionId = request.SessionId,
                UserName = request.Name,
                SubscriptionType = SubscriptionTypeDtoToSubscriptionType(subscriptionType),
            };
        }

        private static SubscriptionTypeDto SubscriptionTypeToSubscriptionTypeDto(SubscriptionType subscriptionType) => subscriptionType switch
        {
            SubscriptionType.Vorangemeldet => SubscriptionTypeDto.Vorangemeldet,
            SubscriptionType.Angemeldet => SubscriptionTypeDto.Angemeldet,
            SubscriptionType.Warteschlange => SubscriptionTypeDto.Warteschlange,
            SubscriptionType.Ohne => SubscriptionTypeDto.Ohne,
            _ => throw new NotImplementedException(),
        };

        private static SubscriptionType SubscriptionTypeDtoToSubscriptionType(SubscriptionTypeDto subscriptionType) => subscriptionType switch
        {
            SubscriptionTypeDto.Vorangemeldet => SubscriptionType.Vorangemeldet,
            SubscriptionTypeDto.Angemeldet => SubscriptionType.Angemeldet,
            SubscriptionTypeDto.Warteschlange => SubscriptionType.Warteschlange,
            SubscriptionTypeDto.Ohne => SubscriptionType.Ohne,
            _ => throw new NotImplementedException(),
        };
    }
}
