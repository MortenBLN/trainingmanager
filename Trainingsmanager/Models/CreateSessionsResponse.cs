namespace Trainingsmanager.Models
{
    public class CreateSessionsResponse
    {
        public List<CreateSessionResponse> Sessions { get; set; } = new();
    }
}
