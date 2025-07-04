document.addEventListener("DOMContentLoaded", async() =>
{
    const form = document.getElementById("create-session-template-form");
    const messageP = document.getElementById("session-message");

    setEndTimeValidator();

    form.addEventListener("submit", async (e) =>
    {
        e.preventDefault();

        const startTime = document.getElementById('trainingStart').value; // "HH:MM"
        const endTime = document.getElementById('trainingEnd').value;    

        const fixedDate = getTodayDateString();
        const startDateTime = getUTCDateTimeString(fixedDate, startTime);
        const endDateTime = getUTCDateTimeString(fixedDate, endTime);

        const data =
        {
            templateName: document.getElementById("templatename").value.trim(),
            teamname: document.getElementById("teamname").value.trim(),
            trainingStart: startDateTime,
            trainingEnd: endDateTime,
            applicationsLimit: parseInt(document.getElementById("applicationsLimit").value),
            applicationsRequired: parseInt(document.getElementById("applicationsRequired").value),
            sessionVenue: document.getElementById("venue").value.trim()
        };

        try
        {
            var token = localStorage.getItem("jwt_token");
            const res = await fetch("/admin/createSessionTemplate",
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
                messageP.textContent = "Nur Admins können Session-Templates erstellen.";
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

            const sessionTemplate = await res.json();

            messageP.textContent = `Template erstellt! Name: ${sessionTemplate.templateName}`;
            messageP.style.color = "green";
            form.reset();

            // Clear any previously added button
            const existingBtn = document.getElementById("code-created-button");
            if (existingBtn) existingBtn.remove();

            // Create and insert the new button
            const btn = document.createElement("button");
            btn.id = "code-created-button";
            btn.textContent = "Jetzt Session erstellen";
            btn.className = "btn btn-success mt-3";
            btn.type = "button"; // important: doesn't submit the form
            btn.onclick = () =>
            {
                window.location.href = `create-session.html`;
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

    function setEndTimeValidator()
    {
        const form = document.getElementById("create-session-template-form");
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

    document.getElementById('trainingStart').addEventListener('focus', function ()
    {
        this.showPicker && this.showPicker(); // For browsers supporting showPicker()
    });

    document.getElementById('trainingEnd').addEventListener('focus', function ()
    {
        this.showPicker && this.showPicker(); // For browsers supporting showPicker()
    });

    // Called in create-session-template.html
    window.adjustTime = function (dateTimeBaseInput, dateTimeToAddToInput, minutesToAdd)
    {
        const baseInput = document.getElementById(dateTimeBaseInput);
        const receivingInput = document.getElementById(dateTimeToAddToInput);

        if (!baseInput || !baseInput.value) return;

        // Parse "HH:MM" from the base input (time only)
        const [startHour, startMinute] = baseInput.value.split(':').map(Number);

        // Create a Date object for today at base time
        const baseDate = new Date();
        baseDate.setHours(startHour, startMinute, 0, 0);

        // Add the minutes
        baseDate.setMinutes(baseDate.getMinutes() + minutesToAdd);

        // Format back to "HH:MM" string
        const endTime = baseDate.toTimeString().slice(0, 5);

        // Check that endTime is after startTime (considering wrap-around)
        // Convert times to minutes from midnight for comparison
        const startTotalMinutes = startHour * 60 + startMinute;
        const endTotalMinutes = (baseDate.getHours() * 60) + baseDate.getMinutes();

        if (endTotalMinutes <= startTotalMinutes)
        {
            // Option 1: alert user
            alert("Endzeit muss nach der Startzeit liegen.");
        }

        receivingInput.value = endTime;
    };

    function getTodayDateString()
    {
        const today = new Date();
        const yyyy = today.getFullYear();
        const mm = String(today.getMonth() + 1).padStart(2, '0'); // Months are 0-based
        const dd = String(today.getDate()).padStart(2, '0');
        return `${yyyy}-${mm}-${dd}`;
    }

    function getUTCDateTimeString(dateString, timeString)
    {
        // dateString: 'YYYY-MM-DD'
        // timeString: 'HH:MM'

        // Create a Date object in local time
        const [year, month, day] = dateString.split('-').map(Number);
        const [hours, minutes] = timeString.split(':').map(Number);

        // Note: month is zero-based in JS Date
        const localDate = new Date(year, month - 1, day, hours, minutes, 0);

        // Convert to ISO string in UTC (e.g. '2025-07-04T12:30:00.000Z')
        return localDate.toISOString();
    }
});
