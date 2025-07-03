namespace Trainingsmanager.Models
{
    public class GetAllSessionTemplatesResponse
    {
        public List<GetSessionTemplateResponse> SessionTemplates { get; set; } = new();
    }
}
