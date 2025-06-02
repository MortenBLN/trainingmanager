using Trainingsmanager.Database.Enums;

namespace Trainingsmanager.Models.Register
{
    public class RegisterRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public RoleEnum Role { get; set; }
    }
}
