using Trainingsmanager.Database.Enums;

namespace Trainingsmanager.Models.DTOs
{
    public class AppUserDto
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public RoleEnum? Role { get; set; }
        public List<SessionDto> Session { get; set; } = new();
        public List<SessionDto> CreatedSessions { get; set; } = new();
    }
}
