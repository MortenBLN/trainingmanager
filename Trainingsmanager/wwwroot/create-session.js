document.addEventListener("DOMContentLoaded", () =>
{
    const form = document.getElementById("create-session-form");
    const messageP = document.getElementById("session-message");
    const weeksInput = document.getElementById("weeksInAdvance");
    const groupnameContainer = document.getElementById("groupname-container");

    setEndTimeValidator();
    setMitgliederToggle();
    createAndToggleGroupNameInput();

    weeksInput.addEventListener("input", createAndToggleGroupNameInput);

    form.addEventListener("submit", async (e) =>
    {
        e.preventDefault();

        const data =
        {
            teamname: document.getElementById("teamname").value.trim(),
            trainingStart: new Date(document.getElementById("trainingStart").value).toISOString(),
            trainingEnd: new Date(document.getElementById("trainingEnd").value).toISOString(),
            applicationsLimit: parseInt(document.getElementById("applicationsLimit").value),
            applicationsRequired: parseInt(document.getElementById("applicationsRequired").value),
            preAddMitglieder: document.getElementById("includeVips").checked,
            countSessionsToCreate: parseInt(document.getElementById("weeksInAdvance").value),
            sessionGruppenName: document.getElementById("groupname") != null ? document.getElementById("groupname").value.trim() : null,
            sessionVenue: document.getElementById("venue").value.trim()
        };

        try
        {
            var token = localStorage.getItem("jwt_token");
            const res = await fetch("/admin/createSession",
                {
                    method: "POST",
                    headers:
                    {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${token}`
                    },
                    body: JSON.stringify(data)
                });
            if (res.status === 401)
            {
                messageP.textContent = "Nur Admins können Sessions erstellen.";
                messageP.style.color = "red";

                const existingBtn = document.getElementById("code-created-button");
                if (existingBtn) existingBtn.remove();

                // Create and insert the new button
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

                throw new Error("Nur Admins können Sessions erstellen."); // Stop further execution
            }
            if (!res.ok)
            {
                const errorData = await res.json();
                const msg = errorData.errors?.generalErrors?.[0] || errorData.message || "Unknown error";
                throw new Error(msg);
            }

            const response = await res.json();

            // Get the value of the "first" Session
            const session = response.sessions?.[0];

            messageP.textContent = `Session(s) erstellt! ID: ${session.id}`;
            messageP.style.color = "green";
            form.reset();
            createAndToggleGroupNameInput();

            // Clear any previously added button
            const existingBtn = document.getElementById("code-created-button");
            if (existingBtn) existingBtn.remove();

            // Create and insert the new button
            const btn = document.createElement("button");
            btn.id = "code-created-button";
            btn.textContent = "Öffne erstellte Session";
            btn.className = "btn btn-success mt-3";
            btn.type = "button"; // important: doesn't submit the form
            btn.onclick = () =>
            {
                window.location.href = `session.html?id=${session.id}`;
            };

            form.appendChild(btn);

            // Create and insert overview button
            const btnOverview = document.createElement("button");
            btnOverview.id = "code-created-button";
            btnOverview.textContent = "Zur Übersicht";
            btnOverview.className = "btn btn-success mt-3";
            btnOverview.type = "button"; // important: doesn't submit the form
            btnOverview.onclick = () =>
            {
                window.location.href = `trainingsessions.html`;
            };

            form.appendChild(btnOverview);
        } catch (err)
        {
            console.error("Create session error:", err);
            messageP.textContent = err.message;
            messageP.style.color = "red";
        }
    });

    function setMitgliederToggle()
    {
        document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => new bootstrap.Tooltip(el));

        // Toggle label update
        const vipsToggle = document.getElementById('includeVips');
        const vipsLabel = document.getElementById('vips-label');

        vipsToggle.addEventListener('change', () =>
        {
            vipsLabel.textContent = vipsToggle.checked ? 'Ja' : 'Nein';
        });
    }

    function setEndTimeValidator()
    {
        const form = document.getElementById("create-session-form");
        const trainingStart = document.getElementById("trainingStart");
        const trainingEnd = document.getElementById("trainingEnd");
        const message = document.getElementById("session-message");

        // Set minimum end time on start time change
        trainingStart.addEventListener("change", () =>
        {
            trainingEnd.min = trainingStart.value;
            validateEndTime(); // validate immediately when start changes
        });

        // Validate live when end time changes
        trainingEnd.addEventListener("change", validateEndTime);

        // Also run check during form submission
        form.addEventListener("submit", (e) =>
        {
            if (!validateEndTime())
            {
                e.preventDefault(); // prevent submit if validation fails
            }
        });

        function validateEndTime()
        {
            const start = new Date(trainingStart.value);
            const end = new Date(trainingEnd.value);

            if (trainingStart.value && trainingEnd.value && end <= start)
            {
                message.textContent = "⚠️ Das Enddatum muss nach dem Startdatum liegen.";
                message.style.color = "red";
                trainingEnd.focus();
                return false;
            }
            else
            {
                message.textContent = "";
                return true;
            }
        }
    }

    function createAndToggleGroupNameInput()
    {
        const value = parseInt(weeksInput.value, 10);

        // Hide or show the container itself
        if (value > 1)
        {
            groupnameContainer.style.display = "block";

            // Only create input if it doesn't exist
            if (!document.getElementById("groupDiv"))
            {
                const groupDiv = document.createElement("div");
                groupDiv.id = "groupDiv";
                groupDiv.className = "mb-3";
                groupDiv.innerHTML = `
                <label for="groupname" class="form-label">Gruppenname</label>
                <input type="text" class="form-control" id="groupname" required />
            `;
                groupnameContainer.appendChild(groupDiv);
            }
        }
        else
        {
            groupnameContainer.style.display = "none";

            // Remove input if it exists
            const groupDiv = document.getElementById("groupDiv");
            if (groupDiv)
            {
                groupDiv.remove();
            }
        }
    }

    // Called in create-session.html
    window.adjustTime = function (dateTimeBaseInput, dateTimeToAddToInput, minutesToAdd)
    {
        const baseInput = document.getElementById(dateTimeBaseInput);
        const receivingInput = document.getElementById(dateTimeToAddToInput);

        if (!baseInput || !baseInput.value) return;

        const baseDate = new Date(baseInput.value);
        baseDate.setMinutes(baseDate.getMinutes() + minutesToAdd);

        receivingInput.value = formatDateToLocalDatetimeString(baseDate);
    };

    function formatDateToLocalDatetimeString(date)
    {
        const pad = (num) => String(num).padStart(2, '0');

        const year = date.getFullYear();
        const month = pad(date.getMonth() + 1);
        const day = pad(date.getDate());
        const hours = pad(date.getHours());
        const minutes = pad(date.getMinutes());

        return `${year}-${month}-${day}T${hours}:${minutes}`;
    }
});
