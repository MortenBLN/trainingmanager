using FastEndpoints;
using Trainingsmanager.Models;
using Trainingsmanager.Services;

namespace Trainingsmanager.Controllers
{
    public class GetAllSessionsEndpoint : EndpointWithoutRequest<GetAllSessionsResponse>
    {
        private readonly ISessionService _service;

        public GetAllSessionsEndpoint(ISessionService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Get("/api/getSessions");
            AllowAnonymous();
        }

        public override async Task<GetAllSessionsResponse> ExecuteAsync(CancellationToken ct)
        {
            return await _service.GetAllSessionsAsync(ct);
        }
    }
}
