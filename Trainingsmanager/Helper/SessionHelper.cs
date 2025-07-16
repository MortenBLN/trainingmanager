using Trainingsmanager.Models;

namespace Trainingsmanager.Helper
{
    public class SessonHelper : ISessionHelper
    {
        public CreateSessionRequest AddWeeksToDates(CreateSessionRequest request, int weeksToAdd, CancellationToken ct)
        {
            var weeksInDays = WeeksToAddInDays(weeksToAdd);

            var requestWithAddedDays = new CreateSessionRequest
            {
                Teamname = request.Teamname,
                ApplicationsLimit = request.ApplicationsLimit,
                ApplicationsRequired = request.ApplicationsRequired,
                PreAddMitglieder = request.PreAddMitglieder,
                CountSessionsToCreate = request.CountSessionsToCreate,
                SessionGruppenName = request.SessionGruppenName,
                SessionVenue = request.SessionVenue,
                MitgliederOnlySession = request.MitgliederOnlySession,

                // Set the new Training start and end to the value of i*7
                TrainingStart = request.TrainingStart.AddDays(weeksInDays),
                TrainingEnd = request.TrainingEnd.AddDays(weeksInDays)
            };

            return requestWithAddedDays;
        }

        private static int WeeksToAddInDays(int weeksToAdd)
        { 
            return weeksToAdd * 7;
        }
    }
}
