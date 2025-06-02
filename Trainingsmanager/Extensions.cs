using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

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
    }

    public class TokenUser
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }
}
