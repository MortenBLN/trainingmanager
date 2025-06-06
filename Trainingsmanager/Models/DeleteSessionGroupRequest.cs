namespace Trainingsmanager.Models
{
    public class DeleteSessionGroupRequest
    {
        // If deleted via one specific Session --> Get GroupId from Session
        public Guid? SessionId { get; set; }
        public Guid? GroupId { get; set; }
    }
}
