using System.Security.Claims;
using Trainingsmanager.Database.Models;
using Trainingsmanager.Models;

namespace Trainingsmanager.Mappers
{
    public interface ISessionTemplateMapper
    {
        SessionTemplate CreateSessionTemplateRequestToSessionTemplate(CreateSessionTemplateRequest request, ClaimsPrincipal user, CancellationToken ct);
        CreateSessionTemplateResponse SessionTemplateToCreateSessionTemplateResponse(SessionTemplate template, CancellationToken ct);
        GetSessionTemplateResponse SessionTemplateToGetSessionTemplateResponse(SessionTemplate template, CancellationToken ct);
        GetAllSessionTemplatesResponse SessionTemplatesToGetAllSessionTemplatesResponse(List<SessionTemplate> templates, CancellationToken ct);
    }
}
