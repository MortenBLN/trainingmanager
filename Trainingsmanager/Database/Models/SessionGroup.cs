namespace Trainingsmanager.Database.Models
{
    public class SessionGroup
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Optional: Description or metadata
        public string? Description { get; set; }

        // Navigation property for related sessions
        public List<Session> Sessions { get; set; } = new();
        public string? SessionGruppenName { get; set; }
    }
}
