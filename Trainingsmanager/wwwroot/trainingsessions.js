// Load sessions and add to the DOM
async function loadSessions()
{
    try
    {
        const res = await fetch("/api/getSessions", { cache: "no-store" });
        const data = await res.json();

        const loginButton = document.getElementById("login-button");
        const weekdayNames = ["Sonntag", "Montag", "Dienstag", "Mittwoch", "Donnerstag", "Freitag", "Samstag"];

        function updateLoginUI()
        {
            const token = localStorage.getItem("jwt_token");

            if (token)
            {
                loginButton.textContent = "Logout";
                loginButton.classList.remove("btn-outline-primary");
                loginButton.classList.add("btn-danger");

                loginButton.onclick = async () =>
                {
                    await fetch("/auth/logout", { method: "POST" });
                    localStorage.removeItem("jwt_token");
                    window.location.reload();
                };
            } else
            {
                loginButton.textContent = "🔐 Login";
                loginButton.classList.remove("btn-danger");
                loginButton.classList.add("btn-outline-primary");

                loginButton.onclick = () =>
                {
                    window.location.href = "login.html";
                };
            }
        }

        updateLoginUI();

        const sessions = data.sessions;
        const now = new Date();

        const upcomingSessions = sessions.filter(s => new Date(s.trainingStart) >= now);

        // Only show expired sessions that are max one week expired
        const oneWeekAgo = new Date(now);
        oneWeekAgo.setDate(now.getDate() - 7);

        const expiredSessions = sessions.filter(s =>
        {
            const sessionDate = new Date(s.trainingStart);
            return sessionDate < now && sessionDate >= oneWeekAgo;
        });

        upcomingSessions.sort((a, b) => new Date(a.trainingStart) - new Date(b.trainingStart));
        expiredSessions.sort((a, b) => new Date(b.trainingStart) - new Date(a.trainingStart));

        generateWeekdayFilterButtons(upcomingSessions, expiredSessions);
        renderSessionsFilteredByWeekday(upcomingSessions, null); // show all by default

        if (expiredSessions.length > 0)
        {
            const list = document.getElementById("session-list");
            const separator = document.createElement('li');
            separator.className = 'session-separator text-center text-muted font-italic my-2';
            separator.textContent = '— Vergangene Sessions —';
            list.appendChild(separator);

            expiredSessions.forEach(session =>
            {
                addSessionToList(session, list, true);
            });
        }

        const btnCreate = document.getElementById("go-create");
        btnCreate.addEventListener("click", () =>
        {
            window.location.href = "create-session.html";
        });
    } catch (err)
    {
        console.error("Failed to load sessions", err);
    }
}

document.addEventListener("DOMContentLoaded", loadSessions);

function generateWeekdayFilterButtons(sessions, expiredSessions)
{
    const filterDiv = document.getElementById("weekday-filter");
    if (!filterDiv) return;
    filterDiv.innerHTML = "";

    // Apply center and spacing style
    filterDiv.classList.add("d-flex", "justify-content-center", "flex-wrap", "gap-2", "mb-3");

    const weekdayNames = ["Sonntag", "Montag", "Dienstag", "Mittwoch", "Donnerstag", "Freitag", "Samstag"];
    const weekdaySet = new Set();
    sessions.forEach(session =>
    {
        const day = new Date(session.trainingStart).getDay();
        weekdaySet.add(day);
    });

    const sortedDays = [...weekdaySet].sort((a, b) => a - b);

    sortedDays.forEach(day =>
    {
        const btn = document.createElement("button");
        btn.className = "btn btn-outline-primary btn-sm text-center";
        btn.style.width = "90px";  // fixed width for all buttons
        btn.style.height = "36px"; // fixed height to keep consistent size
        btn.style.borderRadius = "0"; 
        btn.style.margin = "0 6px 6px 0"// fixed height to keep consistent size
        btn.textContent = weekdayNames[day];
        btn.dataset.day = day;

        btn.addEventListener("click", () =>
        {
            document.querySelectorAll("#weekday-filter button").forEach(b => b.classList.remove("active"));
            btn.classList.add("active");

            const list = document.getElementById("session-list");
            list.innerHTML = "";
            renderSessionsFilteredByWeekday(sessions, parseInt(day));
        });

        filterDiv.appendChild(btn);
    });

    // 'Alle' button
    const allBtn = document.createElement("button");
    allBtn.className = "btn btn-outline-secondary btn-sm text-center px-3";
    allBtn.textContent = "Alle";
    allBtn.style.width = "90px";  // fixed width for all buttons
    allBtn.style.height = "36px";
    allBtn.style.margin = "0 6px 6px 0"// fixed height to keep consistent size
    allBtn.style.borderRadius = "0"; 
    allBtn.addEventListener("click", () =>
    {
        document.querySelectorAll("#weekday-filter button").forEach(b => b.classList.remove("active"));
        allBtn.classList.add("active");
        const list = document.getElementById("session-list");
        list.innerHTML = "";
        renderSessionsFilteredByWeekday(sessions, null);

        // Also render expired Sessions
        if (expiredSessions.length > 0)
        {
            const list = document.getElementById("session-list");
            const separator = document.createElement('li');
            separator.className = 'session-separator text-center text-muted font-italic my-2';
            separator.textContent = '— Vergangene Sessions —';
            list.appendChild(separator);

            expiredSessions.forEach(session =>
            {
                addSessionToList(session, list, true);
            });
        }
    });
    filterDiv.appendChild(allBtn);
}


function renderSessionsFilteredByWeekday(sessions, selectedDay)
{
    const list = document.getElementById("session-list");
    list.innerHTML = "";
    let lastMonthYear = "";

    const filtered = selectedDay == null ? sessions : sessions.filter(session =>
    {
        return new Date(session.trainingStart).getDay() === selectedDay;
    });

    filtered.forEach(session =>
    {
        const date = new Date(session.trainingStart);
        const monthYear = date.getFullYear() + "-" + String(date.getMonth() + 1).padStart(2, '0');

        if (monthYear !== lastMonthYear)
        {
            lastMonthYear = monthYear;
            const monthName = date.toLocaleString('de-DE', { month: 'long', year: 'numeric' });
            const separator = document.createElement('li');
            separator.className = 'session-separator text-center text-muted font-italic my-2';
            separator.textContent = `— ${monthName} —`;
            list.appendChild(separator);
        }

        addSessionToList(session, list, false);
    });
} // end




document.addEventListener("DOMContentLoaded", loadSessions);

function addSessionToList(session, list, expired)
{
    var token = localStorage.getItem("jwt_token");
    var isAdmin = false;

    if (token != null)
    {
        isAdmin = getHasAdminRole(token);
    }

    const subscriptions = Array.isArray(session.subscriptions) ? session.subscriptions : [];
    const isFull = subscriptions.length >= session.applicationsLimit;

    var freeSpotsCount = session.applicationsLimit - session.subscriptions.length;

    var groupName = "";

    if (session.sessionGruppenName != null && session.sessionGruppenName != "")
    {
        groupName = " - " + session.sessionGruppenName;
    }

    // Do not show negative spot count
    if (freeSpotsCount < 0)
    {
        freeSpotsCount = 0;
    }

    const icon = isFull ? "<i class=\"fa fa-times-circle xicon\"></i>" : "<i class=\"fa fa-check-circle checkicon\"></i>"

    const start = new Date(session.trainingStart);
    const end = new Date(session.trainingEnd);
    const formattedDate = formatDate(start);

    // if admin, show hours, if not show start-endtime
    var timeStringToShow;

    if (isAdmin)
    {
        const durationMs = end - start;

        // Convert to hours
        const durationHours = durationMs / (1000 * 60 * 60);

        // round to 1 decimal place
        timeStringToShow = Math.round(durationHours * 10) / 10 + "h";
    }
    else
    {
        const formattedStart = start.toLocaleString("de-DE", {
            hour: "numeric", minute: "numeric"
        });

        const formattedEnd = end.toLocaleString("de-DE", {
            hour: "numeric", minute: "numeric"
        });

        timeStringToShow = `${formattedStart}-${formattedEnd}`;
    }

  
    const li = document.createElement('li');
    li.className = 'd-flex justify-content-between align-items-center';

    const leftSide = `
    <div class="d-flex flex-row align-items-center">
        ${icon}
        <div class="ml-2">
            <h6 class="mb-0 text-truncate" style="max-width: 180px;" title="${session.teamname + groupName}">
              ${(session.teamname + groupName).length > 16 ? (session.teamname + groupName).substring(0, 16) + "…" : (session.teamname + groupName)}
            </h6>
            <div class="d-flex flex-row mt-1 text-black-50 date-time" style="font-size: 0.75rem;">
                <div>
                    <i class="fa fa-calendar-o" style="font-size: 0.75rem;"></i>
                    <span style="margin-left: 2px;">${formattedDate}</span>
                </div>
                <div style="margin-left: 10px;">
                    <i class="fa fa-clock-o" style="font-size: 0.75rem;"></i>
                    <span>${timeStringToShow}</span>
                </div>
            </div>
        </div>
    </div>`;

    const rightSide = document.createElement('div');
    rightSide.className = 'd-flex flex-row align-items-center';

    const info = document.createElement('div');
    info.className = 'd-flex flex-column mr-2';
    info.innerHTML = `<div><i class="fa fa-users"></i><span class="ml-2">${freeSpotsCount} frei</span></div>`;

    if (freeSpotsCount === 0)
    {
        const bottomRightLabel = document.createElement('small');
        bottomRightLabel.textContent = "Warteliste offen";
        bottomRightLabel.className = "position-absolute text-muted text-nowrap"; // <-- prevent wrapping
        bottomRightLabel.style.bottom = "-18px";
        bottomRightLabel.style.right = "-5px";
        bottomRightLabel.style.fontSize = "0.67rem"; // manual size

        // Ensure parent is position: relative
        rightSide.classList.add("position-relative");

        rightSide.appendChild(bottomRightLabel);
    }

    rightSide.appendChild(info);

    // Add Delete & edit button if user is Admin
    if (isAdmin)
    {
        const hasGroup = session.sessionGroupId != null && session.sessionGroupId !== "";

        const deleteBtn = document.createElement('button');
        deleteBtn.className = 'btn btn-danger btn-sm ml-1';
        deleteBtn.innerHTML = '<i class="fa fa-trash"></i>';
        deleteBtn.title = "Session löschen";
        deleteBtn.onclick = (e) =>
        {
            e.stopPropagation();

            if (!hasGroup)
            {
                // Normal confirm dialog for sessions without a group
                showDeleteSingleSessionChoiceDialog(session.id, token, session.teamname);
            }
            else
            {
                // Custom popup for group/session delete choice
                showDeleteChoiceDialog(session.id, session.sessionGruppenName, token);
            }
        };

        rightSide.appendChild(deleteBtn);

        const editBtn = document.createElement('button');
        editBtn.className = 'btn btn-warning btn-sm ml-1';
        editBtn.innerHTML = '<i class="fa fa-pencil"></i>';
        editBtn.title = "Session bearbeiten";
        editBtn.addEventListener('click', (e) =>
        {
            e.stopPropagation(); // 🔒 Prevent the parent li click handler
            window.location.href = `/updatesession.html?id=${session.id}`;
        });

        rightSide.appendChild(editBtn);

        // Display Createsession Button
        document.getElementById("create-session-button-div").style.display = "block";
    }    

    li.innerHTML = leftSide;
    li.appendChild(rightSide);

    if (expired)
    {
        li.classList.add('expired');
    }

    li.addEventListener('click', () => {
        window.location.href = `/session.html?id=${session.id}`;
    });
    list.appendChild(li);
}

function deleteSession(sessionId, token)
{
    fetch(`/api/deletesession`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify({ sessionId })
    }).then(res =>
    {
        if (res.ok)
        {
            loadSessions();
        } else
        {
            handleUnauthorized(res);
        }
    });
}

function deleteSessionGroup(sessionId, token)
{
    fetch(`/api/deleteSessionGroup`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify({ sessionId })
    }).then(res =>
    {
        if (res.ok)
        {
            loadSessions();
        } else
        {
            handleUnauthorized(res);
        }
    });
}

function showDeleteChoiceDialog(sessionId, groupName, token)
{
    // Basic custom modal (or improve with Bootstrap modal if available)
    const modal = document.createElement('div');
    modal.className = 'custom-dialog';
    modal.innerHTML = `
        <div class="custom-dialog-box">
            <p>Was möchtest du löschen?</p>
            <button id="delete-single" class="btn btn-danger btn-sm mt-2">Nur diese Session</button>
            <button id="delete-group" class="btn btn-warning btn-sm mt-2 ml-2">Gesamte Gruppe '${groupName}'</button>
            <button id="cancel" class="btn btn-secondary btn-sm mt-2 ml-2">Abbrechen</button>
        </div>
    `;
    document.body.appendChild(modal);

    document.getElementById('delete-single').onclick = () =>
    {
        deleteSession(sessionId, token);
        modal.remove();
    };
    document.getElementById('delete-group').onclick = () =>
    {
        deleteSessionGroup(sessionId, token);
        modal.remove();
    };
    document.getElementById('cancel').onclick = () =>
    {
        modal.remove();
    };
}

function showDeleteSingleSessionChoiceDialog(sessionId, token, teamname)
{
    // Basic custom modal (or improve with Bootstrap modal if available)
    const modal = document.createElement('div');
    modal.className = 'custom-dialog';
    modal.innerHTML = `
        <div class="custom-dialog-box">
            <p>Löschen von '${teamname}' bestätigen</p>
            <button id="delete-single" class="btn btn-danger btn-sm mt-2">Session löschen</button>
            <button id="cancel" class="btn btn-secondary btn-sm mt-2 ml-2">Abbrechen</button>
        </div>
    `;
    document.body.appendChild(modal);

    document.getElementById('delete-single').onclick = () =>
    {
        deleteSession(sessionId, token);
        modal.remove();
    };
    document.getElementById('cancel').onclick = () =>
    {
        modal.remove();
    };
}

function handleUnauthorized(res)
{
    if (res.status === 401)
    {
        alert("Nur Admins können Sessions löschen.");

        const form = document.getElementById("redirect-button-container");
        const existingBtn = document.getElementById("code-created-button");
        if (existingBtn) existingBtn.remove();

        const loginButton = document.createElement("button");
        loginButton.id = "code-created-button";
        loginButton.textContent = "Logge dich ein";
        loginButton.className = "btn btn-success mt-3";
        loginButton.type = "button";
        loginButton.onclick = () =>
        {
            window.location.href = `login.html`;
        };

        form.appendChild(loginButton);
    }
}

function getHasAdminRole(token)
{
    try
    {
        const base64Url = token.split('.')[1]; // Get payload
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(
            atob(base64)
                .split('')
                .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                .join('')
        );

        var parsedToken = JSON.parse(jsonPayload);
        let isAdmin = false;

        if (parsedToken && parsedToken.role)
        {
            // Some identity providers return a string or array for roles
            if (Array.isArray(parsedToken.role))
            {
                isAdmin = parsedToken.role.includes("Admin");
            }
            else
            {
                isAdmin = parsedToken.role === "Admin";
            }

            return isAdmin;
        }
    } catch (e)
    {
        console.error("Invalid JWT:", e);
        return false;
    }
}

function formatDate(date)
{
    const options = {
        weekday: 'short',
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
    };
    return new Intl.DateTimeFormat('de-DE', options).format(date).replace(',', '');
}
