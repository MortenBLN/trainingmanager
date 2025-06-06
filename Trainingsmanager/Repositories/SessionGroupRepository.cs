using Microsoft.EntityFrameworkCore;
using Trainingsmanager.Database;
using Trainingsmanager.Database.Models;

namespace Trainingsmanager.Repositories
{
    public class SessionGroupRepository : ISessionGroupRepository
    {
        private readonly Context _context;

        public SessionGroupRepository(Context context)
        {
            _context = context;
        }

        public async Task<SessionGroup?> CreateSessionGroupAsync(List<Session> sessions, CancellationToken ct)
        {
            if (sessions == null || sessions.Count == 0)
            {
                throw new NotSupportedException("Session list cannot be null or empty.");
            }

            // Step 1: Create and save group
            var sessionGroup = new SessionGroup
            {
                Name = sessions.First().Teamname + " Gruppe"
            };

            var response = await _context.SessionGroups.AddAsync(sessionGroup, ct);
            await _context.SaveChangesAsync(ct); // ensures sessionGroup.Id is set

            // Step 2: Associate existing sessions with the new group
            foreach (var session in sessions)
            {
                session.SessionGroupId = sessionGroup.Id;
            }
            await _context.SaveChangesAsync(ct);

            // Step 3: Save changes to update sessions

            return response.Entity;
        }

        public async Task DeleteSessionAsync(Guid groupId, CancellationToken ct)
        {
            var sessionGroupToRemove = await _context.SessionGroups.Where(s => s.Id == groupId).FirstOrDefaultAsync(ct);
            if (sessionGroupToRemove == null)
            {
                throw new Exception($"No Sessiongroup with the the ID: '{groupId}' could be found");
            }

            _context.SessionGroups.Remove(sessionGroupToRemove);
            await _context.SaveChangesAsync(ct);
        }
    }
}
