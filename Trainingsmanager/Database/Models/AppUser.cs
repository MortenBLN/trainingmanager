using Trainingsmanager.Database.Enums;

namespace Trainingsmanager.Database.Models
{
    public class AppUser
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public RoleEnum? Role { get; set; }
        public List<Session> Session { get; set; } = new();
        public List<Session> CreatedSessions { get; set; } = new();
    }
}
