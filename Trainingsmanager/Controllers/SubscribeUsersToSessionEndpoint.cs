using FastEndpoints;
using Trainingsmanager.Models;
using Trainingsmanager.Services;

namespace Trainingsmanager.Controllers
{
    public class SubscribeUsersToSessionEndpoint : Endpoint<SubscribeUsersToSessionRequest>
    {
        private readonly ISubscriptionService _service;

        public SubscribeUsersToSessionEndpoint(ISubscriptionService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Post("/api/addUsersToSession");
            AllowAnonymous();
        }

        public override async Task HandleAsync(SubscribeUsersToSessionRequest req, CancellationToken ct)
        {
            try
            {
                await _service.SubscribeToSessionAsync(req, ct);
            }
            catch(InvalidOperationException ex)
            {
                ThrowError(ex.Message);
            }
            await SendNoContentAsync(ct);
        }
    }
}
