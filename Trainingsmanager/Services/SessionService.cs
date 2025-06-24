using Microsoft.Extensions.Options;
using Trainingsmanager.Database.Enums;
using Trainingsmanager.Database.Models;
using Trainingsmanager.Helper;
using Trainingsmanager.Mappers;
using Trainingsmanager.Models;
using Trainingsmanager.Options;
using Trainingsmanager.Repositories;

namespace Trainingsmanager.Services
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _repository;
        private readonly ISessionMapper _mapper;
        private readonly IUserService _userService;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ISessionGroupRepository _sessionGroupRepository;
        private readonly ISessionHelper _helper;
        private readonly ISubscriptionMapper _subscriptionMapper;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<ISessionService> _logger;

        private readonly List<string> _fixedPreAddMitglieder;

        public SessionService (ISessionRepository repository, ISessionMapper mapper, IUserService userService, ISubscriptionRepository subscriptionRepository, IOptions<FixedSubsOptions> options, ISessionHelper helper, ISessionGroupRepository sessionGroupRepository, ISubscriptionMapper subscriptionMapper, ISubscriptionService subscriptionService, ILogger<ISessionService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _userService = userService;
            _subscriptionRepository = subscriptionRepository;
            _helper = helper;
            _sessionGroupRepository = sessionGroupRepository;
            _subscriptionMapper = subscriptionMapper;
            _subscriptionService = subscriptionService;

            _fixedPreAddMitglieder = options.Value.FixedSubs;
            _logger = logger;
        }

        public async Task<CreateSessionsResponse> CreateSessionAsync(CreateSessionRequest request,  CancellationToken ct)
        {
            if (_userService.User == null)
            {
                throw new ArgumentNullException("Es konnte kein angemeldeter Nutzer ermittlert werden.");
            }

            // ApplicationsRequired must be higher or equal to Fixed Mitglieder
            if (request.PreAddMitglieder && request.ApplicationsLimit < _fixedPreAddMitglieder.Count)
            {
                throw new ArgumentException($"Das Limit muss beim Erstellen mit vorgebuchten Mitgliedern mindestens {_fixedPreAddMitglieder.Count} betragen.");
            }

            var createdSessionsResponse = new CreateSessionsResponse();
            var createdSessionResponse = new CreateSessionResponse();

            var createdSessionsAsSessions = new List<Session>();

            // Creates a list of Sessions
            // First session --> i = 0; second session i = 1...
            for (int i = 0; i < request.CountSessionsToCreate; i++)
            {
                // Adds "i" weeks to the Start and Enddate
                var sessionWithAddedWeekDays = _helper.AddWeeksToDates(request, i, ct);

                var createdSession = await MapAndCreateSessions(sessionWithAddedWeekDays, ct);
                createdSessionsAsSessions.Add(createdSession);

                createdSessionResponse = _mapper.SessionToCreateSessionResponse(createdSession, ct);

                createdSessionsResponse.Sessions.Add(createdSessionResponse);
            }

            if (createdSessionsResponse.Sessions.Count > 1)
            {
                await _sessionGroupRepository.CreateSessionGroupAsync(createdSessionsAsSessions, ct);
            }

            _logger.LogInformation(_userService.User.ToTokenUser().Name + $" created {request.Teamname}", DateTime.UtcNow);

            return createdSessionsResponse;
        }

        public async Task DeleteSessionAsync(DeleteSessionRequest req, CancellationToken ct)
        {
            await _repository.DeleteSessionAsync(req.SessionId, ct);
        }

        public async Task<GetAllSessionsResponse> GetAllSessionsAsync(CancellationToken ct)
        {
            var getAllSessionsRepsonse = await _repository.GetAllSessions(ct);
            return _mapper.ListOfSessionToListOfCreateSessionResponse(getAllSessionsRepsonse, ct);
        }

        public async Task<GetSessionResponse> GetSessionByIdAsync(Guid sessionId, CancellationToken ct)
        {
            var session = await _repository.GetSessionByIdAsync(sessionId, ct);

            return _mapper.SessionToGetSessionResponse(session, ct);
        }

        public async Task<GetSessionResponse?> UpdateSessionAsync(UpdateSessionRequest request, CancellationToken ct)
        {
            var sessionToUpdate = await _repository.GetSessionByIdAsync(request.Id, ct);
            var oldLimit = sessionToUpdate.ApplicationsLimit;

            var nonQueueSubscriptions = sessionToUpdate.Subscriptions
                                        .Where(s => s.SubscriptionType != SubscriptionType.Warteschlange)
                                        .Count();

            if (request.ApplicationsLimit < nonQueueSubscriptions)
            {
                throw new ArgumentException($"Das Limit muss mindestens die Anzahl an angemeldeten Teilnahmen ({nonQueueSubscriptions}) betragen.");
            }

            var sessionToUpdateWithNewValues = _mapper.UpdateSessionRequestToSession(request, sessionToUpdate, ct);
            var updatedSession = await _repository.UpdateSessionAsync(sessionToUpdateWithNewValues, ct);

            if (updatedSession == null)
            {
                throw new Exception("An error occured while trying to update the session.");
            }

            // If the applicationLimit is increased
            // --> Check if there are any 'Warteschlangen' Subscriptions, that now need to be updgraded
            if (request.ApplicationsLimit > oldLimit)
            {
                // The amount of 'Warteschlangen' Subscriptions that can be upgraded
                var newFreeSlots = request.ApplicationsLimit - oldLimit;

                var queuedSubscriptions = updatedSession.Subscriptions
                    .Where(s => s.SubscriptionType == SubscriptionType.Warteschlange)
                    .OrderBy(s => s.SubscribedAt)
                    .Take(newFreeSlots)
                    .ToList();

                foreach (var subscription in queuedSubscriptions)
                {
                    // Send mail to each user that subscribed to mail and is in Queue
                    await _subscriptionService.UpgradeSubscription(subscription, updatedSession, ct);
                }
            }

            updatedSession = await _repository.GetSessionByIdAsync(request.Id, ct);

            return _mapper.SessionToGetSessionResponse(updatedSession, ct);
        }

        private async Task<Session> MapAndCreateSessions(CreateSessionRequest request, CancellationToken ct)
        {
            if (_userService.User == null)
            {
                throw new ArgumentException("Es konnte kein eingeloggter Benutzer ermittelt werden.");
            }

            var sessionToCreate = _mapper.CreateSessionRequestToSession(request, _userService.User, ct);

            var createdSession = await _repository.CreateSessionAsync(sessionToCreate, ct);

            // Add the Gründungsmitglieder to each Session
            if (request.PreAddMitglieder)
            {
                foreach (var name in _fixedPreAddMitglieder)
                {
                    var mitgliederSubRequest = new SubscribeUserToSessionRequest
                    {
                        SessionId = createdSession.Id,
                        Name = name,
                    };
                    var preAppliedSubscription = _subscriptionMapper.SubscribeUserToSessionRequestToSession(mitgliederSubRequest, Models.Enums.SubscriptionTypeDto.Vorangemeldet, ct);
                    await _subscriptionRepository.SubscribeToSessionAsync(preAppliedSubscription, ct);
                }

                createdSession = await _repository.GetSessionByIdAsync(createdSession.Id, ct);
            }

            return createdSession;
        }
    }
}
