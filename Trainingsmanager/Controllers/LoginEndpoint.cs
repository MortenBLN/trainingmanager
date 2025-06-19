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
using Trainingsmanager.Services;

namespace Trainingsmanager.Controllers
{
    public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
    {
        private readonly IAuthService _service;

        public LoginEndpoint(IAuthService service) 
        { 
            _service = service;
        }

        public override void Configure()
        {
            Post("/auth/login");
            AllowAnonymous();
        }

        public override async Task<LoginResponse> ExecuteAsync(LoginRequest req, CancellationToken ct)
        {
            LoginResponse? response = null;
            try
            {
                response = await _service.LoginUserAsync(req, ct);
            }
            catch (Exception ex)
            {
                ThrowError(ex.Message);
            }

            return response;
        }
    }
}
