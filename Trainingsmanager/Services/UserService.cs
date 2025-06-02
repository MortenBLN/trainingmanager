using System.Security.Claims;

namespace Trainingsmanager.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public UserService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public ClaimsPrincipal? User => _contextAccessor.HttpContext?.User;
    }
}
