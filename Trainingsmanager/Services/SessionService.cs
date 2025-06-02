using Microsoft.Extensions.Options;
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
        private readonly ISessionHelper _helper;

        private readonly List<string> _fixedPreAddMitglieder;

        public SessionService (ISessionRepository repository, ISessionMapper mapper, IUserService userService, ISubscriptionRepository subscriptionRepository, IOptions<FixedSubsOptions> options, ISessionHelper helper)
        {
            _repository = repository;
            _mapper = mapper;
            _userService = userService;
            _subscriptionRepository = subscriptionRepository;
            _helper = helper;

            _fixedPreAddMitglieder = options.Value.FixedSubs;
        }

        public async Task<CreateSessionsResponse> CreateSession(CreateSessionRequest request,  CancellationToken ct)
        {
            // ApplicationsRequired must be higher or equal to Fixed Mitglieder
            if (request.ApplicationsLimit < _fixedPreAddMitglieder.Count)
            {
                throw new ArgumentException($"Die Maximalanzahl muss beim Erstellen mit vorgebuchten Mitgliedern mindestens {_fixedPreAddMitglieder.Count} betragen.");
            }

            var createdSessions = new CreateSessionsResponse();
            var createdSession = new CreateSessionResponse();

            // Creates a list of Sessions
            // First session --> i = 0; second session i = 1...
            for (int i = 0; i < request.CountSessionsToCreate; i++)
            {
                // Adds "i" weeks to the Start and Enddate
                var sessionWithAddedWeekDays = _helper.AddWeeksToDates(request, i, ct);

                createdSession = await MapAndCreateSessions(sessionWithAddedWeekDays, ct);

                createdSessions.Sessions.Add(createdSession);
            }

            return createdSessions;
        }

        public async Task<GetAllSessionsResponse> GetAllSessions(CancellationToken ct)
        {
            var getAllSessionsRepsonse = await _repository.GetAllSessions(ct);
            return _mapper.ListOfSessionToListOfCreateSessionResponse(getAllSessionsRepsonse, ct);
        }

        public async Task<GetSessionResponse> GetSessionById(Guid sessionId, CancellationToken ct)
        {
            var session = await _repository.GetSessionByIdAsync(sessionId, ct);

            return _mapper.SessionToGetSessionResponse(session, ct);
        }

        private async Task<CreateSessionResponse> MapAndCreateSessions(CreateSessionRequest request, CancellationToken ct)
        {
            var sessionToCreate = _mapper.CreateSessionRequestToSession(request, _userService.User, ct);

            var createdSession = await _repository.CreateSession(sessionToCreate, ct);

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

            return _mapper.SessionToCreateSessionResponse(createdSession, ct);
        }
    }
}
