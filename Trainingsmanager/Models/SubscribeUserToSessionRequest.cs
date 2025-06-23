namespace Trainingsmanager.Models
{
    public class SubscribeUserToSessionRequest
    {
        public Guid SessionId { get; set; }
        public string? Name { get; set; }
        public string? UpdateMail { get; set; }
    }
}
