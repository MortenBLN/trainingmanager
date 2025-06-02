using System.Security.Claims;

namespace Trainingsmanager.Services
{
    public interface IUserService
    {
        ClaimsPrincipal? User { get; }
    }
}
