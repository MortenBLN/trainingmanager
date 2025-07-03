document.addEventListener("DOMContentLoaded", async() =>
{
    const form = document.getElementById("create-session-template-form");
    const messageP = document.getElementById("session-message");

    setEndTimeValidator();

    form.addEventListener("submit", async (e) =>
    {
        e.preventDefault();

        const data =
        {
            templateName: document.getElementById("templatename").value.trim(),
            teamname: document.getElementById("teamname").value.trim(),
            trainingStart: new Date(document.getElementById("trainingStart").value).toISOString(),
            trainingEnd: new Date(document.getElementById("trainingEnd").value).toISOString(),
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
