using FastEndpoints;
using Trainingsmanager.Models;
using Trainingsmanager.Services;

namespace Trainingsmanager.Controllers
{
    public class UpdateSessionEndpoint : Endpoint<UpdateSessionRequest, GetSessionResponse>
    {
        private readonly ISessionService _service;

        public UpdateSessionEndpoint(ISessionService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Post("/admin/updateSession");
            Roles("Admin");
        }

        public override async Task<GetSessionResponse> ExecuteAsync(UpdateSessionRequest req, CancellationToken ct)
        {
            GetSessionResponse? response = null;
            try
            {
                response = await _service.UpdateSessionAsync(req, ct);
            }
            catch (Exception ex)
            {
                ThrowError(ex.Message);
            }

            if (response == null)
            {
                ThrowError("Session konnte nicht aktualisiert werden.");
            }
            return response;
        }
    }
}
