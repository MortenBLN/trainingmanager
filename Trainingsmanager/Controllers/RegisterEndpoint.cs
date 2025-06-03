using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Trainingsmanager.Database;
using Trainingsmanager.Database.Models;
using Trainingsmanager.Models.Register;

namespace Trainingsmanager.Controllers
{
    public class RegisterEndpoint : Endpoint<RegisterRequest, RegisterResponse>
    {
        private readonly Context context;
        public RegisterEndpoint(Context context) 
        { 
            this.context = context;
        }

        public override void Configure()
        {
            Post("/auth/register");
            AllowAnonymous();
        }

        public override async Task<RegisterResponse> ExecuteAsync(RegisterRequest req, CancellationToken ct)
        {
            var newUser = new AppUser
            {
                Email = req.Email,
                Password = req.Password,
                Role = req.Role
            };

            var hasher = new PasswordHasher<AppUser>();
            var hashedPassword = hasher.HashPassword(newUser, req.Password);

            newUser.Password = hashedPassword;
            context.AppUsers.Add(newUser);
            await context.SaveChangesAsync(ct);

            var res = new RegisterResponse()
            {
                Id = newUser.Id,
                Email = newUser.Email
            };
            return res;
        }
    }
}
