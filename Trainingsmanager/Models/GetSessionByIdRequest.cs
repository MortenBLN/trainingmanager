using Microsoft.AspNetCore.Mvc;

namespace Trainingsmanager.Models
{
    public class GetSessionByIdRequest
    {
        [FromRoute]
        public Guid SessionId { get; set; }
    }
}
