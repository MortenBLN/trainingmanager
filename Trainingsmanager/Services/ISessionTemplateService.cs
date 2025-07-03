using Trainingsmanager.Models;

namespace Trainingsmanager.Services
{
    public interface ISessionTemplateService
    {
        Task<CreateSessionTemplateResponse> CreateSessionTemplateAsync(CreateSessionTemplateRequest request, CancellationToken ct);
        Task<GetSessionTemplateResponse> GetSessionTemplateByTemplateNameAsync(string sessionTemplateName, CancellationToken ct);
        Task<GetAllSessionTemplatesResponse> GetAllSessionTemplatesAsync(CancellationToken ct);
    }
}
