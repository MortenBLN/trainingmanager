using System.Security.Claims;
using Trainingsmanager.Database.Models;
using Trainingsmanager.Models;

namespace Trainingsmanager.Mappers
{
    public interface ISessionMapper
    {
        Session CreateSessionRequestToSession(CreateSessionRequest request, ClaimsPrincipal user, CancellationToken ct);
        CreateSessionResponse SessionToCreateSessionResponse(Session request, CancellationToken ct);
        Session CreateSessionResponseToSession(CreateSessionResponse response, CancellationToken ct);
        GetSessionResponse SessionToGetSessionResponse(Session request, CancellationToken ct);
        GetAllSessionsResponse ListOfSessionToListOfCreateSessionResponse(List<Session> request, CancellationToken ct);
        CreateSessionsResponse ListOfSessionToListCreateSessionsResponse(List<Session> request, CancellationToken ct);
    }
}
