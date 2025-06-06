using Trainingsmanager.Models;
using Trainingsmanager.Repositories;

namespace Trainingsmanager.Services
{
    public class SessionGroupService : ISessionGroupService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly ISessionGroupRepository _sessionGroupRepository;

        public SessionGroupService(ISessionRepository sessionRepository, ISessionGroupRepository sessionGroupRepository)
        {
            _sessionRepository = sessionRepository;        
            _sessionGroupRepository = sessionGroupRepository;
        } 

        public async Task DeleteSessionAsync(DeleteSessionRequest req, CancellationToken ct)
        {
            await _sessionRepository.DeleteSessionAsync(req.SessionId, ct);
        }

        public async Task DeleteSessionGroupAsync(DeleteSessionGroupRequest req, CancellationToken ct)
        {
            var groupToDelete = req.GroupId;

            if (groupToDelete == null)
            {
                if (req.SessionId == null)
                {
                    throw new ArgumentNullException("'SessionId' and 'GroupId' must not both be null.");
                }

                var session = await _sessionRepository.GetSessionByIdAsync((Guid)req.SessionId, ct);
                groupToDelete = session.SessionGroupId;

                if (groupToDelete == null)
                {
                    throw new ArgumentNullException("No corresponding 'GroupId' for the given 'SessionId' could be found.");
                }

                await _sessionGroupRepository.DeleteSessionAsync((Guid)groupToDelete, ct);
            }
        }
    }
}
