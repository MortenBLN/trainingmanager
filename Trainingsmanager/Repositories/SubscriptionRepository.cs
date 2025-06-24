using Microsoft.EntityFrameworkCore;
using Trainingsmanager.Database;
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

        public async Task<Subscription> DeleteSubscriptionAsync(DeleteSubscriptionRequest request, CancellationToken ct)
        {
            var subscriptionToRemove = await _context.Subscriptions.Where(s => s.Id == request.SubscriptionId).FirstOrDefaultAsync(ct);
            if (subscriptionToRemove == null)
            {
                throw new Exception($"No Subscription with the the ID: '{ request.SubscriptionId }' could be found");
            }

            var removedSubscription = _context.Subscriptions.Remove(subscriptionToRemove);
            await _context.SaveChangesAsync(ct);

            // Check if the delete request was successful
            return removedSubscription.Entity;
        }

        public async Task<List<Subscription>> GetSubscriptionsOfSessionBySessionIdAsync(Guid sessionId, CancellationToken ct)
        {
            return await _context.Subscriptions
                         .Where(s => s.SessionId == sessionId) 
                         .ToListAsync(ct);
        }

        public async Task<Subscription> SubscribeToSessionAsync(Subscription subscription, CancellationToken ct)
        {
            var existing = await _context.Subscriptions
                .AnyAsync(s => s.UserName == subscription.UserName && s.SessionId == subscription.SessionId, ct);

            if (existing)
            {
                throw new InvalidOperationException("Es existiert bereits ein Teilnehmer mit dem gleichen Namen.");
            }
  
            var newSubscription = await _context.Set<Subscription>().AddAsync(subscription, ct);
            
            await _context.SaveChangesAsync(ct);

            return newSubscription.Entity;
        }

        public async Task UpgradeSubscriptionTypeAsync(Subscription subscriptionToUpgrade, CancellationToken ct)
        {
            _context.Subscriptions.Update(subscriptionToUpgrade);
            await _context.SaveChangesAsync(ct);
        }
    }
}
