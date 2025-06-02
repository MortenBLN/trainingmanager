using FastEndpoints;
using Trainingsmanager.Models;
using Trainingsmanager.Services;

namespace Trainingsmanager.Controllers
{
    public class DeleteSessionEndpoint : Endpoint<DeleteSessionRequest>
    {
        private readonly ISessionService _service;

        public DeleteSessionEndpoint(ISessionService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Post("/api/deleteSession");
            AllowAnonymous();
        }

        public override async Task HandleAsync(DeleteSessionRequest req, CancellationToken ct)
        {
            // TODO
            await SendNoContentAsync(ct);
        }
    }
}
