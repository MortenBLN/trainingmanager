using Trainingsmanager.Database.Models;

namespace Trainingsmanager.Repositories
{
    public interface ISessionTemplateRepository
    {
        Task<SessionTemplate> CreateSessionTemplateAsync(SessionTemplate request, CancellationToken ct);
        Task<SessionTemplate> GetSessionTemplateByNameAsync(string sessionTemplateName, CancellationToken ct);
        Task<List<SessionTemplate>> GetAllSessionTemplates(CancellationToken ct);
    }
}
