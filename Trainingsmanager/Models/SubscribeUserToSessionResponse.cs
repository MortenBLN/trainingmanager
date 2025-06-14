using Trainingsmanager.Models.Enums;

namespace Trainingsmanager.Models
{
    public class SubscribeUserToSessionResponse
    {
        public Guid SessionId { get; set; }
        public string? Name { get; set; }
        public SubscriptionTypeDto SubscriptionType { get; set; }
    }
}
