using FastEndpoints;
using Trainingsmanager.Models;
using Trainingsmanager.Services;

namespace Trainingsmanager.Controllers
{
    public class DeleteSubscriptionEndpoint : Endpoint<DeleteSubscriptionRequest>
    {
        private readonly ISubscriptionService _service;

        public DeleteSubscriptionEndpoint(ISubscriptionService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Post("/api/deleteSubscription");
            AllowAnonymous();
        }

        public override async Task HandleAsync(DeleteSubscriptionRequest req, CancellationToken ct)
        {
            await _service.DeleteSubscriptionAsync(req, ct);
            await SendNoContentAsync(ct);
        }
    }
}
