// Load sessions and add to the DOM
async function loadSessions()
{
    try
    {
        const res = await fetch("/api/getSessions", { cache: "no-store" });
        const data = await res.json();

        const loginButton = document.getElementById("login-button");

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

        updateLoginUI(); // Call on load or re-call

        const sessions = data.sessions;
        const now = new Date();

        const upcomingSessions = sessions.filter(s => new Date(s.trainingStart) >= now);
        const expiredSessions = sessions.filter(s => new Date(s.trainingStart) < now);

        upcomingSessions.sort((a, b) => new Date(a.trainingStart) - new Date(b.trainingStart));
        expiredSessions.sort((a, b) => new Date(b.trainingStart) - new Date(a.trainingStart));

        const list = document.getElementById("session-list");
        list.innerHTML = ""; // Clear list before adding new items

        upcomingSessions.forEach(session =>
        {
            addSessionToList(session, list, false);
        });

        if (expiredSessions.length > 0)
        {
            const separator = document.createElement('li');
            separator.className = 'session-separator text-center';
            separator.textContent = '— Abgelaufene Sessions —';
            list.appendChild(separator);
        }

        expiredSessions.forEach(session =>
        {
            addSessionToList(session, list, true);
        });

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

// 🔁 Call on initial load
document.addEventListener("DOMContentLoaded", loadSessions);

function addSessionToList(session, list, expired)
{
    const subscriptions = Array.isArray(session.subscriptions) ? session.subscriptions : [];
    const isFull = subscriptions.length >= session.applicationsLimit;

    var freeSpontsCount = session.applicationsLimit - session.subscriptions.length;

    var groupName = "";

    if (session.sessionGruppenName != null && session.sessionGruppenName != "")
    {
        groupName = " - " + session.sessionGruppenName;
    }

    // Do not show negative spot count
    if (freeSpontsCount < 0)
    {
        freeSpontsCount = 0;
    }

    const icon = isFull ? "<i class=\"fa fa-times-circle xicon\"></i>" : "<i class=\"fa fa-check-circle checkicon\"></i>"

    const start = new Date(session.trainingStart);
    const end = new Date(session.trainingEnd);

    const formattedDate = formatDate(start);

    const durationMs = end - start;

    // Convert to hours
    const durationHours = durationMs / (1000 * 60 * 60);

    // Optionally, round to 1 decimal place
    const roundedDuration = Math.round(durationHours * 10) / 10;

    const li = document.createElement('li');
    li.className = 'd-flex justify-content-between align-items-center';

    const leftSide = `
        <div class="d-flex flex-row align-items-center">
            ${icon}
            <div class="ml-2">
                <h6 class="mb-0 text-truncate" style="max-width: 180px;" title="${session.teamname + groupName}">
                  ${(session.teamname + groupName).length > 16 ? (session.teamname + groupName).substring(0, 16) + "…" : (session.teamname + groupName) }
                </h6>
                <div class="d-flex flex-row mt-1 text-black-50 date-time">
                    <div><i class="fa fa-calendar-o"></i><span class="ml-2">${formattedDate}</span></div>
                    <div class="ml-3"><i class="fa fa-clock-o"></i><span class="ml-2">${roundedDuration}h</span></div>
                </div>
            </div>
        </div>`;

    const rightSide = document.createElement('div');
    rightSide.className = 'd-flex flex-row align-items-center';

    const info = document.createElement('div');
    info.className = 'd-flex flex-column mr-2';
    info.innerHTML = `<div><i class="fa fa-users"></i><span class="ml-2">${freeSpontsCount} frei</span></div>`;

    rightSide.appendChild(info);

    var token = localStorage.getItem("jwt_token");

    var isAdmin = false;

    if (token != null)
    {
       isAdmin = getHasAdminRole(token);
    }

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
                if (confirm("Delete this session?"))
                {
                    deleteSession(session.id, token);
                }
            } else
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
    return new Intl.DateTimeFormat('en-GB', options).format(date).replace(',', '');
}
