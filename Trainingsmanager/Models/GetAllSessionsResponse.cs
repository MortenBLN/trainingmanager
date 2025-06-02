namespace Trainingsmanager.Models
{
    public class GetAllSessionsResponse
    {
        public List<GetSessionResponse> Sessions { get; set; } = new();
    }
}
