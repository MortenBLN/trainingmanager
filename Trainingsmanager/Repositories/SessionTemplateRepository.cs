using Microsoft.EntityFrameworkCore;
using Trainingsmanager.Database;
using Trainingsmanager.Database.Models;

namespace Trainingsmanager.Repositories
{
    public class SessionTemplateRepository : ISessionTemplateRepository
    {
        private readonly Context _context;

        public SessionTemplateRepository(Context context)
        {
            _context = context;
        }

        public async Task<SessionTemplate> CreateSessionTemplateAsync(SessionTemplate request, CancellationToken ct)
        {
            var response = await _context.SessionTemplates.AddAsync(request, ct);
            await _context.SaveChangesAsync(ct);

            return response.Entity;
        }

        public async Task<List<SessionTemplate>> GetAllSessionTemplates(CancellationToken ct)
        {
            return await _context.SessionTemplates.ToListAsync(ct);
        }

        public async Task<SessionTemplate> GetSessionTemplateByNameAsync(string sessionTemplateName, CancellationToken ct)
        {
            var response = await _context.SessionTemplates
                .SingleOrDefaultAsync(s => s.TemplateName == sessionTemplateName, cancellationToken: ct);

            if (response == null)
            {
                throw new NullReferenceException($"Es konnte kein Sessiontemplate mit dem Namen '{sessionTemplateName}' gefunden werden.");
            }

            return response;
        }
    }
}
