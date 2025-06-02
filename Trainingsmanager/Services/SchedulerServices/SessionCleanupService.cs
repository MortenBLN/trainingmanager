using System.Text;
using Microsoft.EntityFrameworkCore;
using Trainingsmanager.Database;

namespace Trainingsmanager.Services.SchedulerServices
{
    public class SessionCleanupService
    {
        private readonly Context _context;
        private readonly string _logFilePath;
        private readonly ILogger<SessionCleanupService> _logger;

        public SessionCleanupService(Context context, ILogger<SessionCleanupService> logger)
        {
            _context = context;
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DeletedSessionsLog.txt");
            _logger = logger;
        }

        public async Task DeleteOldSessionsAsync()
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-7);
            var oldSessions = await _context.Sessions
                .Where(s => s.TrainingStart < cutoffDate)
                .ToListAsync();

            if (oldSessions.Count == 0)
            {
                Log("No sessions to delete.");
                return;
            }

            var logBuilder = new StringBuilder();
            logBuilder.AppendLine($"[{DateTime.UtcNow}] Deleting {oldSessions.Count} sessions older than {cutoffDate}:");

            foreach (var session in oldSessions)
            {
                logBuilder.AppendLine($" - Session ID: {session.Id}, Training-Start: {session.TrainingStart}");
            }

            _context.Sessions.RemoveRange(oldSessions);
            await _context.SaveChangesAsync();

            Log(logBuilder.ToString());
        }

        private void Log(string message)
        {
            // Log to Application Insights
            _logger.LogInformation(message);

            // Log to console
            Console.WriteLine(message);

            // Log to file
            File.AppendAllText(_logFilePath, message + Environment.NewLine);
        }
    }
}
