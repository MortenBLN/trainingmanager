using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Trainingsmanager.Database.Enums;
using Trainingsmanager.Database.Models;
using Trainingsmanager.Mappers;
using Trainingsmanager.Models.Login;
using Trainingsmanager.Models.Register;
using Trainingsmanager.Options;
using Trainingsmanager.Repositories;

namespace Trainingsmanager.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repository;
        private readonly IAuthMapper _mapper;
        private readonly string? _jwtToken;

        public AuthService(IAuthRepository repository, IAuthMapper mapper, IOptions<JwtTokenOptions> options) 
        { 
            _repository = repository;
            _mapper = mapper;

            _jwtToken = options.Value.JwtSecret;
        }

        public async Task<LoginResponse> LoginUserAsync(LoginRequest req, CancellationToken ct)
        {
            if (req.Email == null)
            {
                throw new BadHttpRequestException("E-Mail benötigt", StatusCodes.Status400BadRequest);
            }

            var emailToFind = req.Email.Trim().ToLower();
        
            var userFromDb = await _repository.GetAppUserByMailAsync(emailToFind, ct) ?? throw new BadHttpRequestException("Login fehlgeschlagen - falsche E-Mail oder Passwort!", StatusCodes.Status404NotFound);
            var hasher = new PasswordHasher<AppUser>();
            var result = hasher.VerifyHashedPassword(userFromDb, userFromDb.Password, req.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                throw new BadHttpRequestException("Login fehlgeschlagen - falsche E-Mail oder Passwort!", StatusCodes.Status404NotFound);
            }

            var roleAsString = Enum.GetName(typeof(RoleEnum), userFromDb.Role ?? RoleEnum.Gast) ?? "Unknown";

            if (_jwtToken == null)
            {
                throw new BadHttpRequestException("Config data not found", StatusCodes.Status500InternalServerError);
            }

            var jwt = JwtBearer.CreateToken(options =>
            {
                options.SigningKey = _jwtToken;
                options.User.Claims.Add(new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, userFromDb.Id.ToString()));
                options.User.Claims.Add(new Claim(JwtRegisteredClaimNames.Name, userFromDb.Email.ToString()));
                options.User.Roles.Add(roleAsString);
            });

            return new LoginResponse
            {
                JwtToken = jwt,
                Email = userFromDb.Email,
            };
        }

        public async Task<RegisterResponse> RegisterUserAsync(RegisterRequest req, CancellationToken ct)
        {
            var mappedAppUser = _mapper.RegisterRequestToAppUser(req, ct);

            var hasher = new PasswordHasher<AppUser>();
            var hashedPassword = hasher.HashPassword(mappedAppUser, mappedAppUser.Password);

            mappedAppUser.Password = hashedPassword;

            var createdAppuser = await _repository.CreateAppUserAsync(mappedAppUser, ct);

            return _mapper.AppUserToRegisterResponse(createdAppuser, ct);
        }
    }
}
