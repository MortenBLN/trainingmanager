using Trainingsmanager.Database.Enums;
using Trainingsmanager.Mappers;
using Trainingsmanager.Models;
using Trainingsmanager.Models.Enums;
using Trainingsmanager.Repositories;

namespace Trainingsmanager.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _repository;
        private readonly ISubscriptionMapper _mapper;
        private readonly ISessionRepository _sessionRepository;
        private readonly ILogger<ISubscriptionService> _logger;

        public SubscriptionService(ISubscriptionRepository repository, ISubscriptionMapper mapper, ISessionRepository sessionRepository, ILogger<ISubscriptionService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

        public async Task DeleteSubscriptionAsync(DeleteSubscriptionRequest request, CancellationToken ct)
        {
            // Get the session based on the subscriptionId
            var session = await _sessionRepository.GetSessionBySubscriptionIdAsync(request.SubscriptionId, ct);

            if (session == null)
            {
                throw new Exception("The Subscription was not attached to any Session.");
            }

            var removedSubscriptionSuccessful = await _repository.DeleteSubscriptionAsync(request, ct);

            if (removedSubscriptionSuccessful == null)
            {
                throw new Exception("Unexpected error: Subscription deletion failed.");
            }

            // If the removed subscription had the type 'Warteschlange' --> No further actions
            if (removedSubscriptionSuccessful.SubscriptionType == SubscriptionType.Warteschlange)
            {
                _logger.LogInformation($"\nRemoved: {removedSubscriptionSuccessful.UserName} from Waitinglist");
                return;
            }

            // The removed type was NOT 'Warteschlange' 
            // --> Check if there is a Warteschlange Subscription that needs to be upgraded

            // Check if there is any 'Warteschlange' Subscription and if so, get the oldest
            var oldestQueuedSubscription = session.Subscriptions
                .Where(s => s.SubscriptionType == SubscriptionType.Warteschlange)
                .OrderBy(s => s.SubscribedAt)
                .FirstOrDefault();

            // There is no 'Warteschlange' Subscription --> Nothing further to do
            if (oldestQueuedSubscription == null)
            {
                _logger.LogInformation($"\nRemoved: {removedSubscriptionSuccessful.UserName} from Subscriptionlist");
                return;
            }
            _logger.LogInformation($"\n______ REMOVAL WITH FOLLOWING UPGRADE START ______\nRemoved: {removedSubscriptionSuccessful.UserName}\nUpgraded:{oldestQueuedSubscription.UserName} \n______ REMOVAL WITH FOLLOWING UPGRADE END ______");

            await _repository.UpgradeSubscriptionTypeAsync(oldestQueuedSubscription, SubscriptionType.Angemeldet, ct);
        }

        public Task<GetSessionResponse> GetSubscriptionsOfSessionBySessionIdAsync(Guid sessionId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<SubscribeUserToSessionResponse> SubscribeToSessionAsync(SubscribeUserToSessionRequest request, CancellationToken ct)
        {
            if (request.Name == null)
            {
                throw new ArgumentException("Der Name darf nicht leer sein.");
            }

            // Check if max subscription count is already hit
            var session = await _sessionRepository.GetSessionByIdAsync(request.SessionId, ct);

            var subAmount = session.Subscriptions.Count();
            var maxSubAmount = session.ApplicationsLimit;

            SubscriptionTypeDto subType = SubscriptionTypeDto.Angemeldet;

            if (subAmount >= maxSubAmount)
            {
                subType = SubscriptionTypeDto.Warteschlange;
            }

            var subscriptionToAdd = _mapper.SubscribeUserToSessionRequestToSession(request, subType, ct);

            var addedSubscription = await _repository.SubscribeToSessionAsync(subscriptionToAdd, ct);

            return _mapper.SubscriptionToSubscribeUsersToSessionResponse(addedSubscription, ct);
        }
    }
}
