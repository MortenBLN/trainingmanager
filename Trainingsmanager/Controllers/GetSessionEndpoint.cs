using FastEndpoints;
using Trainingsmanager.Models;
using Trainingsmanager.Services;

namespace Trainingsmanager.Controllers
{
    public class GetSessionEndpoint : Endpoint<GetSessionByIdRequest, GetSessionResponse>
    {
        private readonly ISessionService _service;

        public GetSessionEndpoint(ISessionService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Get("/api/getSessionById/{SessionId}");
            AllowAnonymous();
        }

        public override async Task<GetSessionResponse> ExecuteAsync(GetSessionByIdRequest req, CancellationToken ct)
        {
            return await _service.GetSessionById(req.SessionId, ct);
        }
    }
}
