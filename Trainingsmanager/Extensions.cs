using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Trainingsmanager
{
    public static class Extensions
    {
        public static TokenUser ToTokenUser(this ClaimsPrincipal user)
        {
            if (user == null)
            {
                throw new ArgumentException("User cannot be null");
            }

            if (!Guid.TryParse(user.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId))
            {
                throw new ArgumentException("Invalid or missing sub claim");
            }
            var name = user.FindFirstValue(JwtRegisteredClaimNames.Name);

            return name == null ? throw new ArgumentException("User cannot be null")
                : new TokenUser
                {
                    Id = userId,
                    Name = name
                };
        }

        public static bool CheckIfUserNotNullAndHasRole(this ClaimsPrincipal user, string roleToCheck)
        {
            if (user == null)
            {
                return false;
            }

            var role = user.FindFirstValue(ClaimTypes.Role) ?? user.FindFirstValue("role");

            if (role == null)
            {
                return false;
            }

            return role == roleToCheck;
        }
    }

    public class TokenUser
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }
}
