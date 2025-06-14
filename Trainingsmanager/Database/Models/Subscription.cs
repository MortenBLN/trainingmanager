using Trainingsmanager.Database.Enums;

namespace Trainingsmanager.Database.Models
{
    public class Subscription
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = default!; // bind by user name, not ID
        public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Ohne;
        public Guid SessionId { get; set; }
        public Session? Session { get; set; }
        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    }
}
