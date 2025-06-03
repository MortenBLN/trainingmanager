using Trainingsmanager.Models.Enums;

namespace Trainingsmanager.Models.Register
{
    public class RegisterRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public RoleEnumDto Role { get; set; }
    }
}
