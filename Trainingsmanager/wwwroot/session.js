window.addEventListener("DOMContentLoaded", () =>
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
        if (!subscriptions.length)
        {
            return;
        }
        subList.innerHTML = "";
        //subList.innerHTML = "";
        subscriptions.forEach(sub =>
        {
            // Remove Button
            const removeBtn = document.createElement("button");
            removeBtn.textContent = "❌";
            removeBtn.style.marginLeft = "10px";
            removeBtn.style.cursor = "pointer";
            removeBtn.title = `Remove ${sub.userName}`;
            removeBtn.addEventListener("click", () => {
                if (confirm(`Remove subscription for ${sub.userName}?`)) {
                    deleteSubscription(sub.id, sub.userName);
                }
            });

            var subscribedAtDate = new Date(sub.subscribedAt);

            const day = subscribedAtDate.toDateString(); // "Mon Jun 02 2025"
            const hours = subscribedAtDate.getHours().toString().padStart(2, '0');
            const minutes = subscribedAtDate.getMinutes().toString().padStart(2, '0');

            const formattedDate = `${day} ${hours}:${minutes}`;

            const li = document.createElement('li');
            li.className = 'd-flex justify-content-between';
            var subType = sub.subscriptionType; // 1 = Mitglied

            const subscriptionTypeIcon = subType == 1 ? "<i title=\"Mitglied\" class=\"fa fa-user-plus subtypeIcon\"></i>" : "<i class=\"fa fa-user subtypeIcon\"></i>"

            li.innerHTML = `
              <div class="d-flex flex-row align-items-center">
                ` + subscriptionTypeIcon + `
                <div class="ml-2">
                  <h6 class="mb-0">${sub.userName}</h6>
                  <div class="d-flex flex-row mt-1 text-black-50 date-time">
                    <div><i class="fa fa-calendar-o"></i><span class="ml-2">Eingeschrieben am ${formattedDate}</span></div>
                  </div>
                </div>
              </div>
              <div class="d-flex flex-row align-items-center right-content">
              </div>
            `;

            //li.appendChild(removeBtn);
            subList.appendChild(li);
            // Append the button to the correct container
            const rightContent = li.querySelector('.right-content');
            rightContent.appendChild(removeBtn);
        });
    }

    function fetchSession()
    {
        fetch(`/api/getSessionById/${sessionId}`)
            .then(res => res.json())
            .then(session =>
            {
                document.getElementById("teamname").textContent = session.teamname || "Unnamed Team";
                document.getElementById("start").textContent = new Date(session.trainingStart).toLocaleString();
                document.getElementById("end").textContent = new Date(session.trainingEnd).toLocaleString();
                document.getElementById("limit").textContent = session.applicationsLimit;
                document.getElementById("required").textContent = session.applicationsRequired;

                renderSubscriptions(session.subscriptions);
            })
            .catch(err =>
            {
                console.error(err);
                detailsDiv.textContent = "Failed to load session.";
                subList.innerHTML = "";
            });
    }

    form.addEventListener("submit", (e) =>
    {
        e.preventDefault();
        const username = usernameInput.value.trim();
        if (!username)
        {
            return;
        }   

        fetch("/api/addUsersToSession",
        {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                sessionId: sessionId,
                name: username
            })
        })
        .then(async res =>
        {
            if (!res.ok)
            {
                const errorData = await res.json();
                const msg = errorData.errors?.generalErrors?.[0] || errorData.message || "Unknown error";
                throw new Error(msg);
            }
            showToast(`${username} nimmt teil.`);
            messageP.textContent = `${username} nimmt teil.`;
            usernameInput.value = "";
            fetchSession(); // refresh list
        })
        .catch(err =>
        {
            console.error("Teilnahme fehlgeschlagen:", err);
            messageP.textContent = err.message;
            showToast(err.message, "error");
        });
    });

    function showToast(message, type = "success")
    {
        const toast = document.getElementById("toast");
        toast.textContent = message;
        toast.classList.add("show");

        toast.classList.remove("toast-success", "toast-error");

        // Add the appropriate type class
        if (type === "error")
        {
            toast.classList.add("toast-error");
            setTimeout(() =>
            {
                toast.classList.remove("show");
            }, 3000); // Hide after 3 seconds
        } else {
            toast.classList.add("toast-success");
            setTimeout(() =>
            {
                toast.classList.remove("show");
            }, 2000); // Hide after 2 seconds
        }
    }

    function deleteSubscription(subscriptionId, username)
    {
        fetch(`/api/deleteSubscription`,
        {
            method: "POST",
             headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                subscriptionId: subscriptionId
            })
        })
            .then(res => {
                if (!res.ok) {
                    return res.json().then(data => {
                        const msg = data.errors?.generalErrors?.[0] || data.message || "Failed to remove subscription.";
                        throw new Error(msg);
                    });
                }
                showToast(`${username} wurde abgemeldet.`);

                fetchSession(); // Reload the updated list
            })
            .catch(err => {
                console.error("Delete failed:", err);
                messageP.textContent = err.message;
            });
    }

    fetchSession();
});