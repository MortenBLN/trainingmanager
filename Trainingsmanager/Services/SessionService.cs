using Azure.Core;
using Microsoft.Extensions.Options;
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

        private readonly List<string> _fixedPreAddMitglieder;

        public SessionService (ISessionRepository repository, ISessionMapper mapper, IUserService userService, ISubscriptionRepository subscriptionRepository, IOptions<FixedSubsOptions> options, ISessionHelper helper, ISessionGroupRepository sessionGroupRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _userService = userService;
            _subscriptionRepository = subscriptionRepository;
            _helper = helper;
            _sessionGroupRepository = sessionGroupRepository;

            _fixedPreAddMitglieder = options.Value.FixedSubs;
        }

        public async Task<CreateSessionsResponse> CreateSessionAsync(CreateSessionRequest request,  CancellationToken ct)
        {
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

            // 
            if (createdSessionsResponse.Sessions.Count > 1)
            {
                await _sessionGroupRepository.CreateSessionGroupAsync(createdSessionsAsSessions, ct);
            }

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
            var anzahlTeilnahmen = sessionToUpdate.Subscriptions.Count();

            if (request.ApplicationsLimit < anzahlTeilnahmen)
            {
                throw new ArgumentException($"Das Limit muss mindestens die Anzahl an vorhandenen Teilnahmen ({anzahlTeilnahmen}) betragen.");
            }

            var sessionToUpdateWithNewValues = _mapper.UpdateSessionRequestToSession(request, sessionToUpdate, ct);
            var updatedSession = await _repository.UpdateSessionAsync(sessionToUpdateWithNewValues, ct);
            
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
                    var mitgliederSubRequest = new SubscribeUsersToSessionRequest
                    {
                        SessionId = createdSession.Id,
                        Name = name,
                    };
                    await _subscriptionRepository.SubscribeToSessionAsync(mitgliederSubRequest, ct, Database.Enums.SubscriptionType.Mitglied);
                }

                createdSession = await _repository.GetSessionByIdAsync(createdSession.Id, ct);
            }

            return createdSession;
        }
    }
}
