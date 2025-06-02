document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("create-session-form");
    const messageP = document.getElementById("session-message");
    const redirectDiv = document.getElementById("redirect-button-container");

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const data =
        {
            teamname: document.getElementById("teamname").value.trim(),
            trainingStart: new Date(document.getElementById("trainingStart").value).toISOString(),
            trainingEnd: new Date(document.getElementById("trainingEnd").value).toISOString(),
            applicationsLimit: parseInt(document.getElementById("applicationsLimit").value),
            applicationsRequired: parseInt(document.getElementById("applicationsRequired").value),
            includeVips: document.getElementById("includeVips").checked,
            preAddMitglieder: parseInt(document.getElementById("weeksInAdvance").value)
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
                    "Authorization": `Bearer ${token}` // ⬅️ Add this line
                    },
                body: JSON.stringify(data)
                });
            if (res.status === 401) {
                messageP.textContent = "Nur Admins können Sessions erstellen.";
                messageP.style.color = "red";

                // Optional: Redirect to login page
                // window.location.href = "/login.html";

                throw new Error("Nur Admins können Sessions erstellen."); // Stop further execution
            }
            // TODO: Handle errors
            if (!res.ok)
            {
                const errorData = await res.json();
                const msg = errorData.errors?.generalErrors?.[0] || errorData.message || "Unknown error";
                throw new Error(msg);
            }
           

            const response = await res.json();

            // Get the value of the "first" Session
            const session = response.sessions?.[0];

            messageP.textContent = `Session(s) created! ID: ${session.id}`;
            messageP.style.color = "green";
            form.reset();

            // Create redirect button
            const btn = document.createElement("button");
            btn.textContent = "Öffne erstellte Session";
            btn.onclick = () =>
            {
                window.location.href = `session.html?id=${session.id}`;
            };
            btn.style.marginTop = "1em";
            redirectDiv.innerHTML = ""; // clear previous content
            redirectDiv.appendChild(btn);

        } catch (err)
        {
            console.error("Create session error:", err);
            messageP.textContent = err.message;
            messageP.style.color = "red";
        }
    });
});