using FastEndpoints;
using Trainingsmanager.Models;
using Trainingsmanager.Services;

namespace Trainingsmanager.Controllers
{
    public class GetSessionTemplateEndpoint : Endpoint<GetSessionTemplateRequest, GetSessionTemplateResponse>
    {
        private readonly ISessionTemplateService _service;

        public GetSessionTemplateEndpoint(ISessionTemplateService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Get("/api/getSessionTemplateByName/{SessionTemplateName}");
            AllowAnonymous();
        }

        public override async Task<GetSessionTemplateResponse> ExecuteAsync(GetSessionTemplateRequest req, CancellationToken ct)
        {
            if (req.SessionTemplateName == null)
            {
                ThrowError("Template name must not be null");
            }

            return await _service.GetSessionTemplateByTemplateNameAsync(req.SessionTemplateName, ct);
        }
    }
}
