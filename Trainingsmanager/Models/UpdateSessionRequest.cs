namespace Trainingsmanager.Models
{
    public class UpdateSessionRequest
    {
        public Guid Id { get; set; }
        public string? TeamName { get; set; }
        public DateTime TrainingStart { get; set; }
        public DateTime TrainingEnd { get; set; }
        public int ApplicationsLimit { get; set; } = 0;
        public int ApplicationsRequired { get; set; } = 0;
        public string? SessionVenue { get; set; }
    }
}
