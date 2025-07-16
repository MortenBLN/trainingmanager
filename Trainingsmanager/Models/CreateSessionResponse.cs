namespace Trainingsmanager.Models
{
    public class CreateSessionResponse
    {
        public Guid Id { get; set; }
        public string? Teamname {  get; set; }
        public string? Url { get; set; }
        public DateTime TrainingStart { get; set; }
        public DateTime TrainingEnd { get; set; }
        public int ApplicationsLimit { get; set; } = 0;
        public int ApplicationsRequired { get; set; } = 0;
        public Guid CreatedById { get; set; }

        // Possible Group
        public Guid? SessionGroupId { get; set; }
        public string? SessionGruppenName { get; set; }
        public string? SessionVenue { get; set; }
        public bool MitgliederOnlySession { get; set; } = false;
    }
}
