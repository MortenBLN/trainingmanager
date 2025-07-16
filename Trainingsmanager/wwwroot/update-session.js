document.addEventListener("DOMContentLoaded", async () =>
{
    const form = document.getElementById("create-session-form");
    const messageP = document.getElementById("session-message");
    const groupnameContainer = document.getElementById("groupname-container");
    const params = new URLSearchParams(window.location.search);
    const sessionId = params.get("id");

    var session = await fetchSession();
    createAndToggleGroupNameInput(session.sessionGruppenName);

    form.addEventListener("submit", async (e) =>
    {
        e.preventDefault();

        const data =
        {
            id: sessionId,
            trainingStart: new Date(document.getElementById("trainingStart").value).toISOString(),
            trainingEnd: new Date(document.getElementById("trainingEnd").value).toISOString(),
            applicationsLimit: parseInt(document.getElementById("applicationsLimit").value),
            applicationsRequired: parseInt(document.getElementById("applicationsRequired").value),
            sessionVenue: document.getElementById("venue").value,
            teamName: document.getElementById("teamname").value,
            mitgliederOnlySession: document.getElementById('vipsOnly').checked
        };

        try
        {
            var token = localStorage.getItem("jwt_token");
            const res = await fetch("/admin/updateSession",
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
                messageP.textContent = "Nur Admins können Sessions aktualisieren.";
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

                throw new Error("Nur Admins können Sessions aktualisieren."); // Stop further execution
            }
            if (!res.ok)
            {
                const errorData = await res.json();
                const msg = errorData.errors?.generalErrors?.[0] || errorData.message || "Unknown error";
                throw new Error(msg);
            }
            messageP.textContent = "Session erfolgreich aktualisiert!";
            messageP.style.color = "green";

            showToast("Session erfolgreich aktualisiert!")
        } catch (err)
        {
            console.error("Update session error:", err);
            messageP.textContent = err.message;
            messageP.style.color = "red";
        }
    });

    async function fetchSession()
    {
        try
        {
            const res = await fetch(`/api/getSessionById/${sessionId}`);
            const session = await res.json();

            const vipsOnlyLabel = document.getElementById('vipsOnly-label');
            const vipsOnlyCheckbox = document.getElementById('vipsOnly');

            vipsOnlyCheckbox.addEventListener('change', () =>
            {
                vipsOnlyLabel.childNodes[0].nodeValue = vipsOnlyCheckbox.checked ? 'Ja' : 'Nein';
            });

            document.getElementById("teamname").value = session.teamname || "Unnamed Team";
            document.getElementById("trainingStart").value = formatDateToLocalDatetimeString(new Date(session.trainingStart));
            document.getElementById("trainingEnd").value = formatDateToLocalDatetimeString(new Date(session.trainingEnd));
            document.getElementById("applicationsLimit").value = session.applicationsLimit;
            document.getElementById("applicationsRequired").value = session.applicationsRequired;
            document.getElementById("venue").value = session.sessionVenue;
            vipsOnlyCheckbox.checked = session.mitgliederOnlySession;

            vipsOnlyLabel.childNodes[0].nodeValue = vipsOnlyCheckbox.checked ? 'Ja' : 'Nein';
            // Groupname is set in own function

            return session;
        } catch (err)
        {
            console.error(err);
            detailsDiv.textContent = "Failed to load session.";
            subList.innerHTML = "";
        }
    }

    function createAndToggleGroupNameInput(groupname)
    {
        if (groupname === null || groupname === undefined || groupname === "")
        {
            groupnameContainer.style.display = "none";
            return;
        }

        groupnameContainer.style.display = "block";

        const groupDiv = document.createElement("div");
        groupDiv.id = "groupDiv";
        groupDiv.className = "mb-3";
        groupDiv.innerHTML = `
            <label for="groupname" class="form-label">Gruppenname</label>
            <input type="text" class="form-control" id="groupname" value="${groupname}" readonly disabled  />
            `;

        groupnameContainer.appendChild(groupDiv);
    }

    // Called in update-session.html
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

    function showToast(message, type = "success")
    {
        const toast = document.getElementById("toast");
        toast.textContent = message;
        toast.classList.remove("toast-success", "toast-error");
        toast.classList.add("show");

        if (type === "error")
        {
            toast.classList.add("toast-error");
            setTimeout(() => toast.classList.remove("show"), 3500);
        } else
        {
            toast.classList.add("toast-success");
            setTimeout(() => toast.classList.remove("show"), 2500);
        }
    }
});
