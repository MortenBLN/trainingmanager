using Trainingsmanager.Models.DTOs;

namespace Trainingsmanager.Models
{
    public class GetSessionResponse
    {
        public Guid Id { get; set; }
        public string? Teamname { get; set; }
        public string? Url { get; set; }
        public DateTime TrainingStart { get; set; }
        public DateTime TrainingEnd { get; set; }
        public int ApplicationsLimit { get; set; } = 0;
        public int ApplicationsRequired { get; set; } = 0;
        public List<SubscriptionDto> Subscriptions { get; set; } = new();
        public List<AppUserDto> Users { get; set; } = new();
        public Guid CreatedById { get; set; }
        public AppUserDto? CreatedBy { get; set; }
    }
}
