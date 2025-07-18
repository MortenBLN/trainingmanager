﻿using System.Security.Claims;
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
                SessionGruppenName = request.SessionGruppenName,
                SessionVenue = request.SessionVenue,
                MitgliederOnlySession = request.MitgliederOnlySession,
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
                    ApplicationsRequired = session.ApplicationsRequired,
                    CreatedById = session.Id,
                    SessionGroupId = session.SessionGroupId,
                    SessionGruppenName = session.SessionGruppenName,
                    SessionVenue = session.SessionVenue,
                    MitgliederOnlySession = session.MitgliederOnlySession,
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
                    },
                    SessionGroupId = session.SessionGroupId,
                    SessionGruppenName = session.SessionGruppenName,
                    SessionVenue = session.SessionVenue,
                    MitgliederOnlySession = session.MitgliederOnlySession,
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
                CreatedById = request.CreatedById,
                SessionGroupId = request.SessionGroupId,
                SessionGruppenName = request.SessionGruppenName,
                SessionVenue = request.SessionVenue,
                MitgliederOnlySession = request.MitgliederOnlySession,
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
                }).ToList() ?? new List<AppUserDto>(),
                SessionGroupId = request.SessionGroupId,
                SessionGruppenName = request.SessionGruppenName,
                SessionVenue = request.SessionVenue,
                MitgliederOnlySession = request.MitgliederOnlySession,
            };
        }

        public Session CreateSessionResponseToSession(CreateSessionResponse response, CancellationToken ct)
        {
            return new Session
            {
                ApplicationsLimit = response.ApplicationsLimit,
                ApplicationsRequired = response.ApplicationsRequired,
                CreatedById = response.CreatedById,
                Teamname = response.Teamname,
                TrainingStart = response.TrainingStart,
                TrainingEnd = response.TrainingEnd,
                Id = response.Id,
                SessionGruppenName = response.SessionGruppenName,
                SessionVenue = response.SessionVenue,
                MitgliederOnlySession = response.MitgliederOnlySession,
            };
        }

        public Session UpdateSessionRequestToSession(UpdateSessionRequest request, Session sessionToUpdate, CancellationToken ct)
        {
            sessionToUpdate.ApplicationsRequired = request.ApplicationsRequired;
            sessionToUpdate.TrainingStart = request.TrainingStart;
            sessionToUpdate.TrainingEnd = request.TrainingEnd;
            sessionToUpdate.ApplicationsLimit = request.ApplicationsLimit;
            sessionToUpdate.SessionVenue = request.SessionVenue;
            sessionToUpdate.Teamname = request.TeamName;
            sessionToUpdate.MitgliederOnlySession = request.MitgliederOnlySession;

            return sessionToUpdate;
        }

        private static SubscriptionType SubscriptionTypeDtoToSubscriptionType(SubscriptionTypeDto subscriptionTypeDto) => subscriptionTypeDto switch
        {
            SubscriptionTypeDto.Vorangemeldet => SubscriptionType.Vorangemeldet,
            SubscriptionTypeDto.Angemeldet => SubscriptionType.Angemeldet,
            SubscriptionTypeDto.Warteschlange => SubscriptionType.Warteschlange,
            SubscriptionTypeDto.Ohne => SubscriptionType.Ohne,
            _ => throw new NotImplementedException(),
        };

        private static SubscriptionTypeDto SubscriptionTypeToSubscriptionTypeDto(SubscriptionType subscriptionType) => subscriptionType switch
        {
            SubscriptionType.Vorangemeldet => SubscriptionTypeDto.Vorangemeldet,
            SubscriptionType.Angemeldet => SubscriptionTypeDto.Angemeldet,
            SubscriptionType.Warteschlange => SubscriptionTypeDto.Warteschlange,
            SubscriptionType.Ohne => SubscriptionTypeDto.Ohne,
            _ => throw new NotImplementedException(),
        };
    }
}
