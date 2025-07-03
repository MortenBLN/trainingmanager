namespace Trainingsmanager.Database.Models
{
    public class SessionTemplate
    {
        public Guid Id { get; set; }
        public string? Teamname {  get; set; }
        public string? Url { get; set; }
        public DateTime TrainingStart { get; set; }
        public DateTime TrainingEnd { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int ApplicationsLimit { get; set; } = 0;
        public int ApplicationsRequired { get; set; } = 0;
        public Guid CreatedById { get; set; }
        public AppUser? CreatedBy { get; set; }
        public string? SessionVenue { get; set; }
        public string? TemplateName { get; set; }
    }
}
