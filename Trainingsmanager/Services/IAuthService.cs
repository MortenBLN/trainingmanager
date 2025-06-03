using Trainingsmanager.Models.Login;
using Trainingsmanager.Models.Register;

namespace Trainingsmanager.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginUserAsync(LoginRequest req, CancellationToken ct);
        Task<RegisterResponse> RegisterUserAsync(RegisterRequest req, CancellationToken ct);
    }
}
