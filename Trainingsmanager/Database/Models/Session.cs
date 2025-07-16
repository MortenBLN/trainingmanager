namespace Trainingsmanager.Database.Models
{
    public class Session
    {
        public Guid Id { get; set; }
        public string? Teamname {  get; set; }
        public string? Url { get; set; }
        public DateTime TrainingStart { get; set; }
        public DateTime TrainingEnd { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int ApplicationsLimit { get; set; } = 0;
        public int ApplicationsRequired { get; set; } = 0;
        public List<Subscription> Subscriptions { get; set; } = new();  
        public List<AppUser> Users { get; set; } = new();
        public Guid CreatedById { get; set; }
        public AppUser? CreatedBy { get; set; }

        // Possible Group
        public Guid? SessionGroupId { get; set; }
        public SessionGroup? SessionGroup { get; set; }
        public string? SessionGruppenName { get; set; }
        public string? SessionVenue { get; set; }
        public bool MitgliederOnlySession { get; set; } = false;
    }
}
