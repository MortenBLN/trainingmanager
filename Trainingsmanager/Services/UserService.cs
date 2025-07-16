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

        public bool IsInRole(string role)
        {
            var roles = User.Claims
                    .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                    .Select(c => c.Value)
                    .ToList();

            return true;
        }

        public ClaimsPrincipal? User => _contextAccessor.HttpContext?.User;
    }
}
