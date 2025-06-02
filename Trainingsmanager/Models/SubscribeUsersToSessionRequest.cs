namespace Trainingsmanager.Models
{
    public class SubscribeUsersToSessionRequest
    {
        public Guid SessionId { get; set; }
        public string? Name { get; set; }
    }
}
