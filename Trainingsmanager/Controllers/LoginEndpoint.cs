using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.EntityFrameworkCore;
using Trainingsmanager.Database;
using Trainingsmanager.Database.Enums;
using Trainingsmanager.Models.Login;

namespace Trainingsmanager.Controllers.Login
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
            var userFromDb = await context.AppUsers.FirstOrDefaultAsync(x => x.Email.ToUpper() == req.Email.ToUpper()
                                                                        && x.Password == req.Password);
            if(userFromDb is null)
            {
                ThrowError("Login failed", StatusCodes.Status404NotFound);
            }

            var roleAsString = Enum.GetName(typeof(RoleEnum), userFromDb.Role ?? RoleEnum.Gast) ?? "Unknown";

            var jwtSecret = Config["JwtSecret"];
            if (jwtSecret == null)
            {
                ThrowError("Config data not found", StatusCodes.Status404NotFound);
            }

            var jwt = JwtBearer.CreateToken(options =>
            {
                options.SigningKey = Config["JwtSecret"];
                options.User.Claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userFromDb.Id.ToString()));
                options.User.Claims.Add(new Claim(JwtRegisteredClaimNames.Name, userFromDb.Email.ToString()));
                options.User.Roles.Add(roleAsString);

            });

            return new LoginResponse
            {
                JwtToken = jwt  ,
                Email = userFromDb.Email,
            };
        }
    }
}
