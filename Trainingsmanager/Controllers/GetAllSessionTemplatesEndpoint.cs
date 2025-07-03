using FastEndpoints;
using Trainingsmanager.Models;
using Trainingsmanager.Services;

namespace Trainingsmanager.Controllers
{
    public class GetAllSessionTemplatesEndpoint : EndpointWithoutRequest<GetAllSessionTemplatesResponse>
    {
        private readonly ISessionTemplateService _service;

        public GetAllSessionTemplatesEndpoint(ISessionTemplateService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Get("/api/getSessionTemplates");
            AllowAnonymous();
        }

        public override async Task<GetAllSessionTemplatesResponse> ExecuteAsync(CancellationToken ct)
        {
            return await _service.GetAllSessionTemplatesAsync(ct);
        }
    }
}
