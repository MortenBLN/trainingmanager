using Microsoft.AspNetCore.Mvc;

namespace Trainingsmanager.Models
{
    public class GetSessionTemplateRequest
    {
        [FromRoute]
        public string? SessionTemplateName { get; set; }
    }
}
