using Microsoft.AspNetCore.Mvc;

namespace Trainingsmanager.Models
{
    public class DeleteSubscriptionRequest
    {
        [FromRoute]
        public Guid SubscriptionId { get; set; }
    }
}
