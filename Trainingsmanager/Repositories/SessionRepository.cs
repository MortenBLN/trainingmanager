using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Trainingsmanager.Database;
using Trainingsmanager.Database.Models;

namespace Trainingsmanager.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly Context _context;

        public SessionRepository(Context context)
        {
            _context = context;
        }

        public async Task<Session> CreateSession(Session request, CancellationToken ct)
        {
            var response = await _context.Sessions.AddAsync(request, ct);
            await _context.SaveChangesAsync(ct);

            return response.Entity;
        }

        public async Task DeleteSessionAsync(Guid sessionId, CancellationToken ct)
        {
            var sessionToRemove = await _context.Sessions.Where(s => s.Id == sessionId).FirstOrDefaultAsync(ct);
            if (sessionToRemove == null)
            {
                throw new Exception($"No Session with the the ID: '{sessionId}' could be found");
            }

            _context.Sessions.Remove(sessionToRemove);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<List<Session>> GetAllSessions(CancellationToken ct)
        {
            return await _context.Sessions
                .Include(s => s.Subscriptions)
                .ToListAsync(ct);
        }

        public async Task<Session> GetSessionByIdAsync(Guid sessionId, CancellationToken ct)
        {
            var response = await _context.Sessions
                .Include(s => s.Subscriptions)
                .SingleOrDefaultAsync(s => s.Id == sessionId, cancellationToken: ct);

            if (response == null)
            {
                throw new NullReferenceException($"Es konnte keine Session mit der Id {sessionId} gefunden werden.");
            }

            return response;
        }
    }
}
