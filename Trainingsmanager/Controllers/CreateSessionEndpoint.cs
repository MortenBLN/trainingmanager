using FastEndpoints;
using Trainingsmanager.Models;
using Trainingsmanager.Services;

namespace Trainingsmanager.Controllers
{
    public class CreateSessionEndpoint : Endpoint<CreateSessionRequest, CreateSessionsResponse>
    {
        private readonly ISessionService _service;

        public CreateSessionEndpoint(ISessionService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Post("/admin/createSession");
            Roles("Admin");
        }

        public override async Task<CreateSessionsResponse> ExecuteAsync(CreateSessionRequest req, CancellationToken ct)
        {
            CreateSessionsResponse? response = null;
            try
            {
                response = await _service.CreateSession(req, ct);
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.StartsWith("Das Limit muss beim Erstellen mit vorgebuchten Mitgliedern mindestens"))
                {
                    ThrowError(ex.Message);
                }
            }

            if (response == null)
            {
                ThrowError("Session konnte nicht erstellt werden.");
            }
            return response;
        }
    }
}
