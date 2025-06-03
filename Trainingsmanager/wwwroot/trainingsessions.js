// Load sessions and add to the DOM
fetch("/api/getSessions", { cache: "no-store" })
    .then(res => res.json())
    .then(data =>
    {
        const loginButton = document.getElementById("login-button");

        function updateLoginUI() {
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

        updateLoginUI(); // Call on load

        const sessions = data.sessions;
        const now = new Date();

        // Separate upcoming and expired sessions
        const upcomingSessions = sessions.filter(s => new Date(s.trainingStart) >= now);
        const expiredSessions = sessions.filter(s => new Date(s.trainingStart) < now);

        // Sort upcoming by soonest date first
        upcomingSessions.sort((a, b) => new Date(a.trainingStart) - new Date(b.trainingStart));

        // Sort expired by most recent past (optional)
        expiredSessions.sort((a, b) => new Date(b.trainingStart) - new Date(a.trainingStart));

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
            separator.textContent = '— Abgelaufenene Sessions —';
            list.appendChild(separator);
        }

        // Iterate through all upcoming sessions
        expiredSessions.forEach(session =>
        {
            addSessionToList(session, list, true)
        });

        var btnCreate = document.getElementById("go-create");

        btnCreate.addEventListener("click", () =>
        {
            window.location.href = "create-session.html";
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
    const end = new Date(session.trainingEnd);

    const day = start.toDateString(); // "Mon Jun 02 2025"
    const hours = start.getHours().toString().padStart(2, '0');
    const minutes = start.getMinutes().toString().padStart(2, '0');

    const formattedDate = `${day} ${hours}:${minutes}`;

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
                <h6 class="mb-0">${session.teamname}</h6>
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

    // Add Delete button if user is Admin
    if (isAdmin)
    {
        // Show "Create session button"
        var buttonContainer = document.getElementById("create-session-button-div");
        buttonContainer.style.display = "block";

        const deleteBtn = document.createElement('button');
        deleteBtn.className = 'btn btn-danger btn-sm ml-1';
        deleteBtn.innerHTML = '<i class="fa fa-trash"></i>';
        deleteBtn.onclick = (e) =>
        {
            e.stopPropagation(); // prevent redirect on click
            if (confirm("Delete this session?")) {
                fetch(`/api/deletesession`,
                {
                    method: "POST",
                    headers:
                    {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${token}`
                    },
                    body: JSON.stringify({
                        sessionId: session.id
                    })
                })
                    .then(res =>
                    {
                        if (res.ok)
                        {
                            li.remove(); // Remove from DOM
                        } else
                        {
                            if (res.status === 401) {
                                messageP.textContent = "Nur Admins können Sessions löschen.";
                                messageP.style.color = "red";

                                const existingBtn = document.getElementById("code-created-button");
                                if (existingBtn) existingBtn.remove();

                                // Create and insert the new button
                                const loginButton = document.createElement("button");
                                loginButton.id = "code-created-button";
                                loginButton.textContent = "Logge dich ein";
                                loginButton.className = "btn btn-success mt-3";
                                loginButton.type = "button";
                                loginButton.onclick = () => {
                                    window.location.href = `login.html`;
                                };

                                form.appendChild(loginButton);

                                throw new Error("Nur Admins können Sessions löschen."); // Stop further execution
                            }
                        }
                    });
            }
        };
        info.innerHTML = `<div><i class="fa fa-users"></i><span class="ml-2">${freeSpontsCount}</span></div>`;
        rightSide.appendChild(deleteBtn);
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