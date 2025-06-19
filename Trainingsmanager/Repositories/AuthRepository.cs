using Microsoft.EntityFrameworkCore;
using Trainingsmanager.Database;
using Trainingsmanager.Database.Models;

namespace Trainingsmanager.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly Context _context;

        public AuthRepository(Context context)
        {
            _context = context;
        }

        public async Task<AppUser> CreateAppUserAsync(AppUser mappedAppUser, CancellationToken ct)
        {
            var createdUser = await _context.AppUsers.AddAsync(mappedAppUser, ct);
            await _context.SaveChangesAsync(ct);

            return createdUser.Entity;
        }

        public async Task<AppUser> GetAppUserByMailAsync(string emailToFind, CancellationToken ct)
        {
            var appUser = await _context.AppUsers
               .FirstOrDefaultAsync(a => a.Email != null && a.Email.ToLower() == emailToFind, ct);

            if (appUser == null)
            {
                throw new NullReferenceException($"Kein Nutzer mit der E-Mail {emailToFind} gefunden");
            }

            return appUser;
        }
    }
}
