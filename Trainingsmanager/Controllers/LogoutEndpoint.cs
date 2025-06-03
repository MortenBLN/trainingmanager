using FastEndpoints;

namespace Trainingsmanager.Controllers
{
    public class LogoutEndpoint : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Post("/auth/logout");
            AllowAnonymous(); 
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            await SendOkAsync(ct);
        }
    }
}
