using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Trainingsmanager.Database.Enums;
using Trainingsmanager.Database.Models;
using Trainingsmanager.Models;
using Trainingsmanager.Models.DTOs;
using Trainingsmanager.Models.Enums;

namespace Trainingsmanager.Mappers
{
    public class SessionMapper : ISessionMapper
    {
        public Session CreateSessionRequestToSession(CreateSessionRequest request, ClaimsPrincipal user,  CancellationToken ct)
        {
            return new Session
            {
                ApplicationsLimit = request.ApplicationsLimit,
                ApplicationsRequired = request.ApplicationsRequired,
                CreatedById = user.ToTokenUser().Id,
                Teamname = request.Teamname,
                TrainingStart = request.TrainingStart,
                TrainingEnd = request.TrainingEnd,
            };
        }

        public CreateSessionsResponse ListOfSessionToListCreateSessionsResponse(List<Session> request, CancellationToken ct)
        {
            var response = new CreateSessionsResponse
            {
                Sessions = request.Select(session => new CreateSessionResponse
                {
                    Id = session.Id,
                    Teamname = session.Teamname ?? string.Empty,
                    Url = session.Url ?? string.Empty,
                    TrainingStart = session.TrainingStart,
                    TrainingEnd = session.TrainingEnd,
                    ApplicationsLimit = session.ApplicationsLimit,
                    ApplicationsRequired = session.ApplicationsRequired
                }).ToList()
            };

            // Check for cancellation before returning
            ct.ThrowIfCancellationRequested();

            return response;
        }

        public GetAllSessionsResponse ListOfSessionToListOfCreateSessionResponse(List<Session> request, CancellationToken ct)
        {
            var response = new GetAllSessionsResponse
            {
                Sessions = request.Select(session => new GetSessionResponse
                {
                    Id = session.Id,
                    Teamname = session.Teamname ?? string.Empty,
                    Url = session.Url ?? string.Empty,
                    TrainingStart = session.TrainingStart,
                    TrainingEnd = session.TrainingEnd,
                    ApplicationsLimit = session.ApplicationsLimit,
                    ApplicationsRequired = session.ApplicationsRequired,
                    Subscriptions = session.Subscriptions?.Select(sub => new SubscriptionDto
                    {
                        Id = sub.Id,
                        UserName = sub.UserName,
                        SubscribedAt = sub.SubscribedAt,
                        SessionId = sub.SessionId,
                        SubscriptionType = SubscriptionTypeToSubscriptionTypeDto(sub.SubscriptionType),
                    }).ToList() ?? new List<SubscriptionDto>(),
                    Users = session.Users.Select(u => new AppUserDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                    }).ToList(),
                    CreatedById = session.CreatedById,
                    CreatedBy = session.CreatedBy is null ? null : new AppUserDto
                    {
                        Id = session.CreatedBy.Id,
                        Email = session.CreatedBy.Email,
                    }
                }).ToList()
            };

            return response;
        }

        public CreateSessionResponse SessionToCreateSessionResponse(Session request, CancellationToken ct)
        {
            return new CreateSessionResponse
            {
                Id = request.Id,
                ApplicationsLimit = request.ApplicationsLimit,
                ApplicationsRequired = request.ApplicationsRequired,
                Teamname = request.Teamname,
                TrainingStart = request.TrainingStart,
                TrainingEnd = request.TrainingEnd,
            };
        }

        public GetSessionResponse SessionToGetSessionResponse(Session request, CancellationToken ct)
        {
            return new GetSessionResponse
            {
                Id = request.Id,
                ApplicationsLimit = request.ApplicationsLimit,
                ApplicationsRequired = request.ApplicationsRequired,
                Teamname = request.Teamname,
                TrainingStart = request.TrainingStart,
                TrainingEnd = request.TrainingEnd,
                Url = request.Url,
                CreatedById = request.CreatedById,
                Subscriptions = request.Subscriptions?.Select(sub => new SubscriptionDto
                {
                    Id = sub.Id,
                    UserName = sub.UserName,
                    SubscribedAt = sub.SubscribedAt,
                    SessionId = sub.SessionId,
                    SubscriptionType = SubscriptionTypeToSubscriptionTypeDto(sub.SubscriptionType),
                }).ToList() ?? new List<SubscriptionDto>(),

                Users = request.Users?.Select(u => new AppUserDto
                {
                    Id = u.Id,
                    Email = u.Email
                }).ToList() ?? new List<AppUserDto>()
            };
        }

        private static SubscriptionType SubscriptionTypeDtoToSubscriptionType(SubscriptionTypeDto subscriptionTypeDto) => subscriptionTypeDto switch
        {
            SubscriptionTypeDto.Admin => SubscriptionType.Admin,
            SubscriptionTypeDto.Mitglied => SubscriptionType.Mitglied,
            SubscriptionTypeDto.Probe => SubscriptionType.Probe,
            SubscriptionTypeDto.Gast => SubscriptionType.Gast,
            _ => throw new NotImplementedException(),
        };

        private static SubscriptionTypeDto SubscriptionTypeToSubscriptionTypeDto(SubscriptionType subscriptionType) => subscriptionType switch
        {
            SubscriptionType.Admin => SubscriptionTypeDto.Admin,
            SubscriptionType.Mitglied => SubscriptionTypeDto.Mitglied,
            SubscriptionType.Probe => SubscriptionTypeDto.Probe,
            SubscriptionType.Gast => SubscriptionTypeDto.Gast,
            _ => throw new NotImplementedException(),
        };
    }
}
