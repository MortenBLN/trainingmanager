using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Trainingsmanager.Database;
using Trainingsmanager.Database.Enums;
using Trainingsmanager.Database.Models;
using Trainingsmanager.Models.Login;

namespace Trainingsmanager.Controllers
{
    public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
    {
        private readonly Context context;

        public LoginEndpoint(Context context) 
        { 
            this.context = context;
        }

        public override void Configure()
        {
            Post("/auth/login");
            AllowAnonymous();
        }

        public override async Task<LoginResponse> ExecuteAsync(LoginRequest req, CancellationToken ct)
        {
            if (req.Email == null)
            {
                ThrowError("E-Mail benötigt", StatusCodes.Status400BadRequest);
            }

            var emailToFind = req.Email.Trim().ToLower();

            var userFromDb = await context.AppUsers
                .FirstOrDefaultAsync(a => a.Email.ToLower() == emailToFind, ct);

            if (userFromDb is null)
            {
                ThrowError("Login fehlgeschlagen - falsche E-Mail oder Passwort!", StatusCodes.Status404NotFound);
            }

            var hasher = new PasswordHasher<AppUser>();
            var result = hasher.VerifyHashedPassword(userFromDb, userFromDb.Password, req.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ThrowError("Login fehlgeschlagen - falsche E-Mail oder Passwort!", StatusCodes.Status404NotFound);
            }

            var roleAsString = Enum.GetName(typeof(RoleEnum), userFromDb.Role ?? RoleEnum.Gast) ?? "Unknown";

            var jwtSecret = Config["JwtSecret"];

            if (jwtSecret == null)
            {
                ThrowError("Config data not found", StatusCodes.Status500InternalServerError);
            }

            var jwt = JwtBearer.CreateToken(options =>
            {
                options.SigningKey = jwtSecret;
                options.User.Claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userFromDb.Id.ToString()));
                options.User.Claims.Add(new Claim(JwtRegisteredClaimNames.Name, userFromDb.Email.ToString()));
                options.User.Roles.Add(roleAsString);
            });

            return new LoginResponse
            {
                JwtToken = jwt,
                Email = userFromDb.Email,
            };
        }
    }
}
