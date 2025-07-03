using System.Security.Claims;
using Trainingsmanager.Database.Models;
using Trainingsmanager.Models;

namespace Trainingsmanager.Mappers
{
    public class SessionTemplateMapper : ISessionTemplateMapper
    {
        public SessionTemplate CreateSessionTemplateRequestToSessionTemplate(CreateSessionTemplateRequest request, ClaimsPrincipal user, CancellationToken ct)
        {
            return new SessionTemplate
            {
                ApplicationsLimit = request.ApplicationsLimit,
                ApplicationsRequired = request.ApplicationsRequired,
                CreatedById = user.ToTokenUser().Id,
                Teamname = request.Teamname,
                TrainingStart = request.TrainingStart,
                TrainingEnd = request.TrainingEnd,
                TemplateName = request.TemplateName,
                SessionVenue = request.SessionVenue
            };
        }

        public GetAllSessionTemplatesResponse SessionTemplatesToGetAllSessionTemplatesResponse(List<SessionTemplate> templates, CancellationToken ct)
        {
            var response = new GetAllSessionTemplatesResponse
            {
                SessionTemplates = templates.Select(session => new GetSessionTemplateResponse
                {
                    Id = session.Id,
                    Teamname = session.Teamname ?? string.Empty,
                    Url = session.Url ?? string.Empty,
                    TrainingStart = session.TrainingStart,
                    TrainingEnd = session.TrainingEnd,
                    ApplicationsLimit = session.ApplicationsLimit,
                    ApplicationsRequired = session.ApplicationsRequired,
                    TemplateName= session.TemplateName,
                    SessionVenue = session.SessionVenue,
                    CreatedById = session.CreatedById
                }).ToList()
            };

            return response;
        }

        public CreateSessionTemplateResponse SessionTemplateToCreateSessionTemplateResponse(SessionTemplate template, CancellationToken ct)
        {
            return new CreateSessionTemplateResponse
            {
                Id = template.Id,
                ApplicationsLimit = template.ApplicationsLimit,
                ApplicationsRequired = template.ApplicationsRequired,
                Teamname = template.Teamname,
                TrainingStart = template.TrainingStart,
                TrainingEnd = template.TrainingEnd,
                TemplateName = template.TemplateName,
                SessionVenue = template.SessionVenue,
                CreatedById = template.CreatedById
            };
        }

        public GetSessionTemplateResponse SessionTemplateToGetSessionTemplateResponse(SessionTemplate template, CancellationToken ct)
        {
            return new GetSessionTemplateResponse
            {
                Id = template.Id,
                ApplicationsLimit = template.ApplicationsLimit,
                ApplicationsRequired = template.ApplicationsRequired,
                Teamname = template.Teamname,
                TrainingStart = template.TrainingStart,
                TrainingEnd = template.TrainingEnd,
                TemplateName = template.TemplateName,
                SessionVenue = template.SessionVenue,
                CreatedById = template.CreatedById
            };
        }
    }
}
