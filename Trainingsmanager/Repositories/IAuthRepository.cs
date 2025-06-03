using Trainingsmanager.Database.Models;

namespace Trainingsmanager.Repositories
{
    public interface IAuthRepository
    {
        Task <AppUser> CreateAppUserAsync(AppUser mappedAppUser, CancellationToken ct);
        Task<AppUser> GetAppUserByMailAsync(string emailToFind, CancellationToken ct);
    }
}
