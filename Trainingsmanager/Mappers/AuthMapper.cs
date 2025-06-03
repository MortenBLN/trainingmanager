using Trainingsmanager.Database.Enums;
using Trainingsmanager.Database.Models;
using Trainingsmanager.Models.Enums;
using Trainingsmanager.Models.Register;

namespace Trainingsmanager.Mappers
{
    public class AuthMapper : IAuthMapper
    {
        public AppUser RegisterRequestToAppUser(RegisterRequest request, CancellationToken ct)
        {
            return new AppUser
            {
                Email = request.Email,
                Password = request.Password,
                Role = RoleEnumDtoToRoleEnum(request.Role),
            };
        }

        public RegisterResponse AppUserToRegisterResponse(AppUser createdAppuser, CancellationToken ct)
        {
            return new RegisterResponse
            {
                Id = createdAppuser.Id,
                Email = createdAppuser.Email
            };
        }

        private static RoleEnumDto RoleEnumToRoleEnumDto(RoleEnum role) => role switch
        {
            RoleEnum.Admin => RoleEnumDto.Admin,
            RoleEnum.Mitglied => RoleEnumDto.Mitglied,
            RoleEnum.Probe => RoleEnumDto.Probe,
            RoleEnum.Gast => RoleEnumDto.Gast,
            _ => throw new NotImplementedException(),
        };

        private static RoleEnum RoleEnumDtoToRoleEnum(RoleEnumDto role) => role switch
        {
            RoleEnumDto.Admin => RoleEnum.Admin,
            RoleEnumDto.Mitglied => RoleEnum.Mitglied,
            RoleEnumDto.Probe => RoleEnum.Probe,
            RoleEnumDto.Gast => RoleEnum.Gast,
            _ => throw new NotImplementedException(),
        };

    }
}
