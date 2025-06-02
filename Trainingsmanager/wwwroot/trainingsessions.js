// Load sessions and add to the DOM
fetch("/api/getSessions")
    .then(res => res.json())
    .then(data => {
        const sessions = data.sessions;

        const now = new Date();

        // Separate upcoming and expired sessions
        const upcomingSessions = sessions.filter(s => new Date(s.trainingStart) >= now);
        const expiredSessions = sessions.filter(s => new Date(s.trainingStart) < now);

        // Sort upcoming by soonest date first
        upcomingSessions.sort((a, b) => new Date(a.trainingStart) - new Date(b.trainingStart));

        // Sort expired by most recent past (optional)
        expiredSessions.sort((a, b) => new Date(b.trainingStart) - new Date(a.trainingStart));

        // Combine the sorted arrays
        const sortedSessions = [...upcomingSessions, ...expiredSessions];

        const list = document.getElementById("session-list");

        // Iterate through all upcoming sessions
        upcomingSessions.forEach(session =>
        {
            addSessionToList(session, list, false)
        });

        // Add seperator

        if (expiredSessions.length > 0)
        {
            const separator = document.createElement('li');
            separator.className = 'session-separator text-center';
            separator.textContent = '— Expired Sessions —';
            list.appendChild(separator);
        }

        // Iterate through all upcoming sessions
        expiredSessions.forEach(session =>
        {
            addSessionToList(session, list, true)
        });
    })
    .catch(err => console.error("Failed to load sessions", err));

function addSessionToList(session, list, expired)
{
    const isFull = session.subscriptions.length >= session.applicationsLimit;
    var freeSpontsCount = session.applicationsLimit - session.subscriptions.length;

    // Do not show negative spot count
    if (freeSpontsCount < 0)
    {
        freeSpontsCount = 0;
    }

    const icon = isFull ? "<i class=\"fa fa-times-circle xicon\"></i>" : "<i class=\"fa fa-check-circle checkicon\"></i>"

    const start = new Date(session.trainingStart);
    const formattedStart = start.toString().split(' GMT')[0];
    const end = new Date(session.trainingEnd);

    const durationMs = end - start;

    // Convert to hours
    const durationHours = durationMs / (1000 * 60 * 60);

    // Optionally, round to 1 decimal place
    const roundedDuration = Math.round(durationHours * 10) / 10;

    const li = document.createElement('li');
    li.className = 'd-flex justify-content-between';

    li.innerHTML = `
      <div class="d-flex flex-row align-items-center">`
            + icon +
            `<div class="ml-2">
          <h6 class="mb-0">${session.teamname}</h6>
          <div class="d-flex flex-row mt-1 text-black-50 date-time">
            <div><i class="fa fa-calendar-o"></i><span class="ml-2">${formattedStart}</span></div>
            <div class="ml-3"><i class="fa fa-clock-o"></i><span class="ml-2">${roundedDuration}h</span></div>
          </div>
        </div>
      </div>
      <div class="d-flex flex-row align-items-center">
        <div class="d-flex flex-column mr-2">
          <span class="date-time">${freeSpontsCount} spot(s) available</span>
        </div>
        <i class="fa fa-ellipsis-h"></i>
      </div>
    `;

    if (expired)
    {
        li.classList.add('expired');
    }

    li.addEventListener('click', () => {
        window.location.href = `/session.html?id=${session.id}`;
    });
    list.appendChild(li);
}