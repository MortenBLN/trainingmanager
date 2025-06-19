using FastEndpoints;
using Trainingsmanager.Models;
using Trainingsmanager.Services;

namespace Trainingsmanager.Controllers
{
    public class SubscribeUsersToSessionEndpoint : Endpoint<SubscribeUserToSessionRequest, SubscribeUserToSessionResponse>
    {
        private readonly ISubscriptionService _service;

        public SubscribeUsersToSessionEndpoint(ISubscriptionService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Post("/api/addSubscription");
            AllowAnonymous();
        }

        public override async Task<SubscribeUserToSessionResponse> HandleAsync(SubscribeUserToSessionRequest req, CancellationToken ct)
        {
            SubscribeUserToSessionResponse? response = null;

            try
            {
                response = await _service.SubscribeToSessionAsync(req, ct);
            }
            catch(InvalidOperationException ex)
            {
                ThrowError(ex.Message);
            }
            return response;
        }
    }
}
