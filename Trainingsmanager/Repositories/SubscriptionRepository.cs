using Microsoft.EntityFrameworkCore;
using Trainingsmanager.Database;
using Trainingsmanager.Database.Enums;
using Trainingsmanager.Database.Models;
using Trainingsmanager.Models;

namespace Trainingsmanager.Repositories
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly Context _context;

        public SubscriptionRepository(Context context)
        {
            _context = context;
        }

        public async Task DeleteSubscriptionAsync(DeleteSubscriptionRequest request, CancellationToken ct)
        {
            var subscriptionToRemove = await _context.Subscriptions.Where(s => s.Id == request.SubscriptionId).FirstOrDefaultAsync(ct);
            if (subscriptionToRemove == null)
            {
                throw new Exception($"No Subscription with the the ID: '{ request.SubscriptionId }' could be found");
            }

            _context.Subscriptions.Remove(subscriptionToRemove);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<List<Subscription>> GetSubscriptionsOfSessionBySessionIdAsync(Guid sessionId, CancellationToken ct)
        {
            return await _context.Subscriptions
                         .Where(s => s.SessionId == sessionId) 
                         .ToListAsync(ct);
        }

        public async Task SubscribeToSessionAsync(SubscribeUsersToSessionRequest request, CancellationToken ct, SubscriptionType subType = SubscriptionType.Gast)
        {
            var existing = await _context.Subscriptions
                .AnyAsync(s => s.UserName == request.Name && s.SessionId == request.SessionId, ct);

            if (existing)
            {
                throw new InvalidOperationException("Es existiert bereits ein Teilnehmer mit dem gleichen Namen.");
            }

            if (request.Name == null)
            {
                throw new ArgumentException("Der Name darf nicht leer sein.");
            }

            // Add each Sub by the given name
            var newSubscription = new Subscription
            {
                SessionId = request.SessionId,
                UserName = request.Name,
                SubscriptionType = subType
            };

            await _context.Set<Subscription>().AddAsync(newSubscription, ct);
            await _context.SaveChangesAsync(ct);
        }
    }
}
