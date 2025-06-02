using Trainingsmanager.Models;
using Trainingsmanager.Repositories;

namespace Trainingsmanager.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _repository;
        private readonly ISessionRepository _sessionRepository;
        private readonly IUserService _userService;

        public SubscriptionService(ISubscriptionRepository repository, IUserService userService, ISessionRepository sessionRepository)
        {
            _repository = repository;
            _userService = userService;
            _sessionRepository = sessionRepository;
        }

        public async Task DeleteSubscriptionAsync(DeleteSubscriptionRequest request, CancellationToken ct)
        {
            await _repository.DeleteSubscriptionAsync(request, ct);
        }

        public Task<GetSessionResponse> GetSubscriptionsOfSessionBySessionIdAsync(Guid sessionId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task SubscribeToSessionAsync(SubscribeUsersToSessionRequest request, CancellationToken ct)
        {
            // Check if max subscription count is already hit
            var session = await _sessionRepository.GetSessionByIdAsync(request.SessionId, ct);

            var subAmount = session.Subscriptions.Count();
            var maxSubAmount = session.ApplicationsLimit;
            if (subAmount >= maxSubAmount)
            {
                throw new InvalidOperationException("The max sub amount is already reached :(");
            }

            await _repository.SubscribeToSessionAsync(request, ct);
        }
    }
}
