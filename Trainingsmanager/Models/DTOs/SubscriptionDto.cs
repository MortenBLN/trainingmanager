using Trainingsmanager.Models.Enums;

namespace Trainingsmanager.Models.DTOs
{
    public class SubscriptionDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = default!; // bind by user name, not ID
        public SubscriptionTypeDto SubscriptionType { get; set; } = SubscriptionTypeDto.Gast;
        public Guid SessionId { get; set; }
        public SessionDto? Session { get; set; }
        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    }
}
