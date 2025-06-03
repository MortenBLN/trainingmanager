window.addEventListener("DOMContentLoaded", async () =>
{
    const detailsDiv = document.getElementById("session-details");
    const subList = document.getElementById("applicationsList");
    const form = document.getElementById("add-subscription-form");
    const usernameInput = document.getElementById("username-input");
    const messageP = document.getElementById("subscription-message");

    const params = new URLSearchParams(window.location.search);
    const sessionId = params.get("id");

    if (!sessionId)
    {
        detailsDiv.textContent = "No session ID provided in URL.";
        return;
    }

    function renderSubscriptions(subscriptions)
    {
        subList.innerHTML = "";

        if (!subscriptions.length) return;

        subscriptions.forEach(sub =>
        {
            const removeBtn = document.createElement("button");
            removeBtn.textContent = "❌";
            removeBtn.style.marginLeft = "10px";
            removeBtn.style.cursor = "pointer";
            removeBtn.title = `Remove ${sub.userName}`;
            removeBtn.addEventListener("click", () =>
            {
                if (confirm(`Remove subscription for ${sub.userName}?`))
                {
                    deleteSubscription(sub.id, sub.userName);
                }
            });

            const subscribedAtDate = new Date(sub.subscribedAt);
            const day = subscribedAtDate.toDateString();
            const hours = subscribedAtDate.getHours().toString().padStart(2, '0');
            const minutes = subscribedAtDate.getMinutes().toString().padStart(2, '0');
            const formattedDate = `${day} ${hours}:${minutes}`;

            const li = document.createElement('li');
            li.className = 'd-flex justify-content-between';
            const subType = sub.subscriptionType;
            const subscriptionTypeIcon = subType === 1
                ? `<i title="Mitglied" class="fa fa-user-plus subtypeIcon"></i>`
                : `<i class="fa fa-user subtypeIcon"></i>`;

            li.innerHTML = `
              <div class="d-flex flex-row align-items-center">
                ${subscriptionTypeIcon}
                <div class="ml-2">
                  <h6 class="mb-0">${sub.userName}</h6>
                  <div class="d-flex flex-row mt-1 text-black-50 date-time">
                    <div><i class="fa fa-calendar-o"></i><span class="ml-2">Eingeschrieben am ${formattedDate}</span></div>
                  </div>
                </div>
              </div>
              <div class="d-flex flex-row align-items-center right-content"></div>
            `;

            subList.appendChild(li);
            li.querySelector('.right-content').appendChild(removeBtn);
        });
    }

    async function fetchSession()
    {
        try
        {
            const res = await fetch(`/api/getSessionById/${sessionId}`);
            const session = await res.json();

            document.getElementById("teamname").textContent = session.teamname || "Unnamed Team";
            document.getElementById("start").textContent = new Date(session.trainingStart).toLocaleString();
            document.getElementById("end").textContent = new Date(session.trainingEnd).toLocaleString();
            document.getElementById("limit").textContent = session.applicationsLimit + " (" + session.subscriptions.length + " belegt)";
            document.getElementById("required").textContent = session.applicationsRequired;

            renderSubscriptions(session.subscriptions);
            return session;
        } catch (err)
        {
            console.error(err);
            detailsDiv.textContent = "Failed to load session.";
            subList.innerHTML = "";
        }
    }

    form.addEventListener("submit", async (e) =>
    {
        e.preventDefault();
        const username = usernameInput.value.trim();
        if (!username) return;

        try
        {
            const res = await fetch("/api/addUsersToSession", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ sessionId: sessionId, name: username })
            });

            if (!res.ok)
            {
                const errorData = await res.json();
                const msg = errorData.errors?.generalErrors?.[0] || errorData.message || "Unknown error";
                throw new Error(msg);
            }

            const session = await fetchSession();
            const currentCount = document.querySelectorAll("#applicationsList li").length;
            const remaining = session.applicationsLimit - currentCount;

            let freeSpotsString = ``;

            switch (remaining)
            {
                case 0:
                    freeSpotsString = `Das war der letzte freie Platz.`;
                    break;
                case 1:
                    freeSpotsString = `Noch 1 freier Platz`;
                    break;
                default:
                    freeSpotsString = `Noch ${remaining} freie Plätze`;
            }
            showToast(`${username} nimmt teil.\n${freeSpotsString}`);
            messageP.textContent = `${username} nimmt teil.`;
            usernameInput.value = "";

        } catch (err)
        {
            console.error("Teilnahme fehlgeschlagen:", err);
            messageP.textContent = err.message;
            showToast(err.message, "error");
        }
    });

    function showToast(message, type = "success")
    {
        const toast = document.getElementById("toast");
        toast.textContent = message;
        toast.classList.remove("toast-success", "toast-error");
        toast.classList.add("show");

        if (type === "error")
        {
            toast.classList.add("toast-error");
            setTimeout(() => toast.classList.remove("show"), 3000);
        } else
        {
            toast.classList.add("toast-success");
            setTimeout(() => toast.classList.remove("show"), 2000);
        }
    }

    async function deleteSubscription(subscriptionId, username)
    {
        try
        {
            const res = await fetch(`/api/deleteSubscription`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ subscriptionId: subscriptionId })
            });

            if (!res.ok)
            {
                const data = await res.json();
                const msg = data.errors?.generalErrors?.[0] || data.message || "Failed to remove subscription.";
                throw new Error(msg);
            }

            showToast(`${username} wurde abgemeldet.`);
            await fetchSession();
        } catch (err)
        {
            console.error("Delete failed:", err);
            messageP.textContent = err.message;
            showToast(err.message, "error");
        }
    }

    await fetchSession();
});