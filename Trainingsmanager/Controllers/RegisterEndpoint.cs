using FastEndpoints;
using Trainingsmanager.Models.Register;
using Trainingsmanager.Services;

namespace Trainingsmanager.Controllers
{
    public class RegisterEndpoint : Endpoint<RegisterRequest, RegisterResponse>
    {
        private readonly IAuthService _service;

        public RegisterEndpoint(IAuthService authService) 
        { 
            _service = authService;
        }

        public override void Configure()
        {
            Post("/auth/register");
            Roles("Admin");
        }

        public override async Task<RegisterResponse> ExecuteAsync(RegisterRequest req, CancellationToken ct)
        {
            var res = await _service.RegisterUserAsync(req, ct);
            return res;
        }
    }
}
