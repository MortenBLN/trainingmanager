using Trainingsmanager.Database.Models;
using Trainingsmanager.Models.Register;

namespace Trainingsmanager.Mappers
{
    public interface IAuthMapper
    {
        RegisterResponse AppUserToRegisterResponse(AppUser createdAppuser, CancellationToken ct);
        AppUser RegisterRequestToAppUser(RegisterRequest request, CancellationToken ct);
    }
}
