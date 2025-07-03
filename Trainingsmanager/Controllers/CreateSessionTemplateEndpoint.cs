using FastEndpoints;
using Trainingsmanager.Models;
using Trainingsmanager.Services;

namespace Trainingsmanager.Controllers
{
    public class CreateSessionTemplateEndpoint : Endpoint<CreateSessionTemplateRequest, CreateSessionTemplateResponse>
    {
        private readonly ISessionTemplateService _service;

        public CreateSessionTemplateEndpoint(ISessionTemplateService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Post("/admin/createSessionTemplate");
            Roles("Admin");
        }

        public override async Task<CreateSessionTemplateResponse> ExecuteAsync(CreateSessionTemplateRequest req, CancellationToken ct)
        {
            CreateSessionTemplateResponse? response = null;
            try
            {
                response = await _service.CreateSessionTemplateAsync(req, ct);
            }
            catch (Exception ex)
            {     
                ThrowError(ex.Message);
            }

            if (response == null)
            {
                ThrowError("Session Template konnte nicht erstellt werden.");
            }
            return response;
        }
    }
}
