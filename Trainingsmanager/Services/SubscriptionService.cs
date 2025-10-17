using Microsoft.Extensions.Options;
using Trainingsmanager.Database.Enums;
using Trainingsmanager.Database.Models;
using Trainingsmanager.Mappers;
using Trainingsmanager.Models;
using Trainingsmanager.Models.Enums;
using Trainingsmanager.Options;
using Trainingsmanager.Repositories;
using Trainingsmanager.Services.EmailServices;

namespace Trainingsmanager.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _repository;
        private readonly ISubscriptionMapper _mapper;
        private readonly ISessionRepository _sessionRepository;
        private readonly ILogger<ISubscriptionService> _logger;
        private readonly IEmailService _emailService;
        private readonly EMailOptions _emailOptions;
        private readonly IUserService _userService;

        public SubscriptionService(ISubscriptionRepository repository, ISubscriptionMapper mapper, ISessionRepository sessionRepository, ILogger<ISubscriptionService> logger, IEmailService emailService, IOptions<EMailOptions> emailOptions, IUserService userService)
        {
            _repository = repository;
            _mapper = mapper;
            _sessionRepository = sessionRepository;
            _logger = logger;
            _emailService = emailService;
            _emailOptions = emailOptions.Value;
            _userService = userService;
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
                _logger.LogInformation($"\nRemoved: {removedSubscriptionSuccessful.UserName} from Waitinglist in {session.Teamname} with trainingstart at {session.TrainingStart}", DateTime.UtcNow);
                return;
            }

            // As we have more members than our normal max amount of people, we need to ONLY upgrade a subscription, when we actually have space
            // --> Check if amount of NON WARTESCHLANGEN subscriptions is still greater or equal to max number of people before continuing
            session = await _sessionRepository.GetSessionByIdAsync(session.Id, ct);
            var allNonWarteschlangenSubscriptions = session.Subscriptions.Where(s => s.SubscriptionType != SubscriptionType.Warteschlange).ToList();

            if (allNonWarteschlangenSubscriptions.Count >= session.ApplicationsLimit)
            {
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
                _logger.LogInformation($"\nRemoved: {removedSubscriptionSuccessful.UserName} from Subscriptionlist in {session.Teamname} with trainingstart at {session.TrainingStart}", DateTime.UtcNow);
                return;
            }
            _logger.LogInformation($"\n______ REMOVAL WITH FOLLOWING UPGRADE START ______\nRemoved: {removedSubscriptionSuccessful.UserName}\nUpgraded:{oldestQueuedSubscription.UserName} in {session.Teamname} with trainingstart at {session.TrainingStart} \n______ REMOVAL WITH FOLLOWING UPGRADE END ______", DateTime.UtcNow);

            await UpgradeSubscription(oldestQueuedSubscription, session, ct);
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

            // Check if user is Admin (used for some checks)
            bool hasAdminRole = false;

            if  (_userService.User != null)
            {
                hasAdminRole = _userService.User.CheckIfUserNotNullAndHasRole("Admin");
            }
     
            // Only Admins are allowed to add Subscriptions for expired sessions
            if (session.TrainingStart < DateTime.UtcNow)
            {
                if (!hasAdminRole)
                {
                    throw new Exception("Die Session ist bereits abgelaufen, keine Anmeldung möglich.");
                }
            }

            var timeIn3Days = DateTime.UtcNow.AddHours(72);

            if (session.TrainingStart > timeIn3Days)
            {
                if (!hasAdminRole)
                {
                    throw new Exception("Eine Anmeldung ist erst 3 Tage vor Beginn der Session möglich.");
                }
            }

            // Only Admins are allowed add Subscriptions for MitgliederOnlySession
            if (session.MitgliederOnlySession)
            {
                if (!hasAdminRole)
                {
                    throw new Exception("Nur Admins können Mitglieder bei einer 'Nur für Mitglieder' Session hinzufügen");
                }
            }

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

        public async Task UpgradeSubscription(Subscription oldestQueuedSubscription, Session session, CancellationToken ct)
        {
            await SendMail(oldestQueuedSubscription, session);

            var subscriptionToUpgrade = _mapper.SubscriptionToUpgradedSubscription(oldestQueuedSubscription, SubscriptionType.Angemeldet);
            await _repository.UpgradeSubscriptionTypeAsync(subscriptionToUpgrade, ct);
        }

        private async Task SendMail(Subscription oldestQueuedSubscription, Session session)
        {
            // Check if the user added a mail when adding the name to the waitlist --> if so, send mail that the subscription was upgraded
            if (oldestQueuedSubscription.UpdateMail == null || oldestQueuedSubscription.UpdateMail == "")
            {
                return;
            }

            try
            {
                if (session.Teamname == null)
                {
                    throw new Exception("Der Teamname darf zum Senden der Mail nicht null sein.");
                }

                if (_emailOptions.SubscriptionUpgrageMail == null)
                {
                    throw new Exception("Die SubscriptionUpgrageMail Vorlage darf zum Senden der Mail nicht null sein.");
                }

                if (_emailOptions.PathToservice == null)
                {
                    throw new Exception("Der Pfad zum Service darf zum Senden der Mail nicht null sein.");
                }

                string trainingName = session.Teamname;

                // Add Id to the url
                string link = string.Format(_emailOptions.PathToservice, session.Id.ToString());

                // Format the message using string.Format
                string formattedBody = string.Format(
                    _emailOptions.SubscriptionUpgrageMail,
                    oldestQueuedSubscription.UserName,
                    trainingName,
                    link
                );

                await _emailService.SendEmailAsync(oldestQueuedSubscription.UpdateMail!, $"Teilnahme '{trainingName}'", formattedBody);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Exception when trying to send mail: " + ex.Message, DateTime.UtcNow);
            }
        }
    }
}
