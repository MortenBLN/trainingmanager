namespace Trainingsmanager.Services.SchedulerServices
{
    public class SessionCleanupHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public SessionCleanupHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var cleanupService = scope.ServiceProvider.GetRequiredService<SessionCleanupService>();
                    await cleanupService.DeleteOldSessionsAsync();
                }

                await Task.Delay(TimeSpan.FromDays(7), stoppingToken); // Run every 7 days
            }
        }
    }
}
