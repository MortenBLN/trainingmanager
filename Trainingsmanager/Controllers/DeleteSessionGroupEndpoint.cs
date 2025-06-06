using FastEndpoints;
using Trainingsmanager.Models;
using Trainingsmanager.Services;

namespace Trainingsmanager.Controllers
{
    public class DeleteSessionGroupEndpoint : Endpoint<DeleteSessionGroupRequest>
    {
        private readonly ISessionGroupService _service;

        public DeleteSessionGroupEndpoint(ISessionGroupService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Post("/api/deleteSessionGroup");
            Roles("Admin");
        }

        public override async Task HandleAsync(DeleteSessionGroupRequest req, CancellationToken ct)
        {
            await _service.DeleteSessionGroupAsync(req, ct);
        }
    }
}
