using Trainingsmanager.Database.Models;
using Trainingsmanager.Models.DTOs;

namespace Trainingsmanager.Models
{
    public class SubscribeUsersToSessionResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public AppUserDto? User { get; set; }

        public Guid SessionId { get; set; }
        public Session? Session { get; set; }

        public DateTime SubscribedAt { get; set; }
    }
}
