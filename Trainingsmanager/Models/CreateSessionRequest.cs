namespace Trainingsmanager.Models
{
    public class CreateSessionRequest
    {
        public string? Teamname {  get; set; }
        public DateTime TrainingStart { get; set; }
        public DateTime TrainingEnd { get; set; }
        public int ApplicationsLimit { get; set; } = 0;
        public int ApplicationsRequired { get; set; } = 0;
        public bool PreAddMitglieder { get; set; } = true;

        // Used to create the same session for x weeks, every week - might set a limit in FE
        public int CountSessionsToCreate { get; set; } = 1;
        public string? SessionGruppenName { get; set; }
        public string? SessionVenue { get; set; }
        public bool MitgliederOnlySession { get; set; } = false;
    }
}
