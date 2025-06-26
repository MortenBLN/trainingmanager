window.addEventListener("DOMContentLoaded", async () =>
{
    const detailsDiv = document.getElementById("session-details");
    const subList = document.getElementById("validSubscriptionList");
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

    async function renderSubscriptions(subscriptions, sessionIsExpired)
    {
        const validSubscriptionsList = document.getElementById("validSubscriptionList");
        const queuedSubscriptionsList = document.getElementById("queuedSubscriptionList");
        const separator = document.getElementById("subscriptions-separator");
        const separatorHeader = document.getElementById("subscriptions-separator-header");

        // Clear existing content
        validSubscriptionsList.innerHTML = "";
        queuedSubscriptionsList.innerHTML = "";
        separator.style.display = "none";
        separatorHeader.style.display = "none";

        if (!subscriptions.length) return;

        // Split and sort
        const queuedSubs = subscriptions
            .filter(sub => sub.subscriptionType === 2)
            .sort((a, b) => new Date(a.subscribedAt) - new Date(b.subscribedAt));

        const validSubs = subscriptions
            .filter(sub => sub.subscriptionType !== 2)
            .sort((a, b) => new Date(a.subscribedAt) - new Date(b.subscribedAt));

        // Show separator only if both lists have items
        if (queuedSubs.length && validSubs.length)
        {
            separator.style.display = "block";
            separatorHeader.style.display = "block";
        }

        // render lists
        async function renderToList(list, listType, sub, sessionIsExpired, index = null)
        {
            const subscribedAtDate = new Date(sub.subscribedAt);
            const subscribedAtDateFormated = subscribedAtDate.toLocaleString("de-DE", {
                day: "numeric",
                month: "numeric",
                year: "numeric",
                hour: "2-digit",
                minute: "2-digit",
                second: undefined // omit seconds
            });    

            const li = document.createElement('li');
            li.className = 'd-flex justify-content-between';

            // Display index number if provided, otherwise use icon
            const leadingContent = index !== null
                ? `<span class="index-number font-weight-bold mr-2">#${index + 1}</span>`
                : (sub.subscriptionType === 0
                    ? `<i title="Mitglied" class="fa fa-user-plus subtypeIcon"></i>`
                    : `<i class="fa fa-user subtypeIcon"></i>`);

            li.innerHTML = `
              <div class="d-flex flex-row align-items-center">
                ${leadingContent}
                <div class="ml-2">
                  <h6 class="mb-0">${sub.userName}</h6>
                  <div class="d-flex flex-row mt-1 text-black-50 date-time">
                    <div><i class="fa fa-calendar-o"></i><span class="ml-2">Angemeldet: ${subscribedAtDateFormated} Uhr</span></div>
                  </div>
                </div>
              </div>
              <div class="d-flex flex-row align-items-center right-content"></div>
            `;

            list.appendChild(li);

            // Allow delete only when session is not expired yet
            if (!sessionIsExpired)
            {
                const removeBtn = document.createElement("button");
                removeBtn.textContent = "❌";
                removeBtn.style.marginLeft = "10px";
                removeBtn.style.cursor = "pointer";
                removeBtn.style.border = "none";
                removeBtn.style.backgroundColor = "white";
                removeBtn.title = `Remove ${sub.userName}`;
                removeBtn.addEventListener("click", async () =>
                {
                    const confirmed = await showDeleteConfirmationDialog(sub.userName, listType);

                    if (confirmed)
                    {
                        deleteSubscription(sub.id, sub.userName);
                    }
                });

                li.querySelector('.right-content').appendChild(removeBtn);
            }
        }

        // Render both lists
        for (const sub of validSubs)
        {
            await renderToList(validSubscriptionsList, "Teilnehmerliste", sub, sessionIsExpired);
        }

        for (const [index, sub] of queuedSubs.entries())
        {
            await renderToList(queuedSubscriptionsList, "Warteliste", sub, sessionIsExpired, index);
        }
    }

    async function fetchSession()
    {
        try
        {
            const res = await fetch(`/api/getSessionById/${sessionId}`);
            const session = await res.json();
            messageP.textContent = "";

            const validSubsCount = session.subscriptions
                .filter(sub => sub.subscriptionType !== 2).length;

            const queuedSubsCount = session.subscriptions
                .filter(sub => sub.subscriptionType === 2).length;

            var additionalQueuedSubString = "";

            if (queuedSubsCount != null && queuedSubsCount != undefined && queuedSubsCount > 0)
            {             
                additionalQueuedSubString = ` & ${queuedSubsCount} warten`
            }

            var validSubCountString = ` (${validSubsCount} belegt${additionalQueuedSubString})`;

            const dateStart = new Date(session.trainingStart);
            const formattedStart = dateStart.toLocaleString("de-DE", {
                day: "numeric",
                month: "numeric",
                year: "numeric",
                hour: "2-digit",
                minute: "2-digit",
                second: undefined // omit seconds
            });

            const dateEnd = new Date(session.trainingEnd);
            const formattedEnd = dateEnd.toLocaleString("de-DE", {
                day: "numeric",
                month: "numeric",
                year: "numeric",
                hour: "2-digit",
                minute: "2-digit",
                second: undefined // omit seconds
            });

            const now = new Date();
            const sessionIsExpired = dateStart < now;

            document.getElementById("teamname").textContent = session.teamname || "Unnamed Team";
            document.getElementById("start").textContent = `${formattedStart} Uhr`;
            document.getElementById("end").textContent = `${formattedEnd} Uhr`;
            document.getElementById("limit").textContent = session.applicationsLimit + validSubCountString;
            document.getElementById("required").textContent = session.applicationsRequired;

            // Display the venue, make Hyperlink clickable
            if (session.sessionVenue != null && session.sessionVenue != undefined && session.sessionVenue != "")
            {
                renderVenue(session.sessionVenue);
            }

            await renderSubscriptions(session.subscriptions, sessionIsExpired);

            return session;
        }
        catch (err)
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
            // Check if there is a free spot
            var session = await fetchSession();
            var remaining = session.applicationsLimit - session.subscriptions.length
            var mail = null;

            if (remaining <= 0)
            {
                const result = await showEmailPromptDialog(username);

                if (result.continue == false)
                {
                    return;
                }

                mail = result.email;
            }

            const res = await fetch("/api/addSubscription", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ sessionId: sessionId, name: username, updateMail: mail })
            });

            if (!res.ok)
            {
                const errorData = await res.json();
                const msg = errorData.errors?.generalErrors?.[0] || errorData.message || "Unknown error";
                throw new Error(msg);
            }

            session = await fetchSession();
            remaining = session.applicationsLimit - session.subscriptions.length

            let freeSpotsString = ``;

            if (remaining < 0)
            {
                freeSpotsString = `${username} wurde der Warteliste hinzugefügt`;
            }
            else if (remaining === 0)
            {
                freeSpotsString = `${username} nimmt teil.\nDas war der letzte freie Platz.`;
            }
            else if (remaining === 1)
            {
                freeSpotsString = `${username} nimmt teil.\nNoch 1 freier Platz`;
            }
            else
            {
                freeSpotsString = `${username} nimmt teil.\nNoch ${remaining} freie Plätze`;
            }

            showToast(`${freeSpotsString}`);
            messageP.textContent = `${freeSpotsString}.`;
            messageP.style.color = "green";
            usernameInput.value = "";

        }
        catch (err)
        {
            console.error("Teilnahme fehlgeschlagen:", err);
            messageP.style.color = "red";
            messageP.textContent = err.message;
            showToast(err.message, "error");
        }
    });

    function renderVenue(text)
    {
        const urlRegex = /(https?:\/\/[^\s]+)/gi;
        const html = text.replace(urlRegex, url =>
            `<a href="${url}" target="_blank" rel="noopener noreferrer">${url}</a>`
        );
        document.getElementById("venue").innerHTML = html;
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
            messageP.style.color = "red";
            showToast(err.message, "error");
        }
    }

    function showDeleteConfirmationDialog(userName, listType)
    {
        return new Promise((resolve) =>
        {
            // Remove any existing dialog
            const existingDialog = document.querySelector('.custom-dialog');
            if (existingDialog) existingDialog.remove();

            const modal = document.createElement('div');
            modal.className = 'custom-dialog';
            modal.innerHTML = `
            <div class="custom-dialog-box">
                <p>Soll ${userName} aus der ${listType} entfernt werden?</p>
                <button id="confirmDelete" class="btn btn-danger btn-sm mt-2">${userName} entfernen</button>
                <button id="cancelDelete" class="btn btn-warning btn-sm mt-2 ml-2">Nicht entfernen</button>
            </div>
        `;
            document.body.appendChild(modal);

            document.getElementById('confirmDelete').onclick = () =>
            {
                modal.remove();
                resolve(true);
            };

            document.getElementById('cancelDelete').onclick = () =>
            {
                modal.remove();
                resolve(false);
            };
        });
    }

    function showEmailPromptDialog(userName)
    {
        return new Promise((resolve) =>
        {
            // Remove existing dialog if present
            const existingDialog = document.querySelector('.custom-dialog');
            if (existingDialog) existingDialog.remove();

            const modal = document.createElement('div');
            modal.className = 'custom-dialog';
            modal.innerHTML = `
                <div class="custom-dialog-box text-center">
                    <p>Hallo ${userName}, du stehst auf der Warteliste.</p>
                    <p>Möchtest du benachrichtigt werden, sobald ein Platz für dich frei wird?</p>

                    <div class="d-flex flex-column align-items-center mt-2" style="gap: 20px;">
                        <input type="email" id="emailPromptInput" class="form-control form-control-sm" 
                               placeholder="email@example.com"                
                               style="width: 290px; background-color: #f9f9f9; border: 3px solid #ccc; box-shadow: 0 0 4px rgba(0,0,0,0.1); border-radius: 4px;" />
                        <div class="d-flex" style="gap: 10px;">
                            <button id="emailConfirm" class="btn btn-success btn-sm" style="width: 140px; height:50px;">Benachrichtige mich!</button>
                            <button id="emailSkip" class="btn btn-secondary btn-sm" style="width: 140px; height:50px;">Ohne E-Mail fortfahren</button>
                        </div>
                    </div>

                    <div class="d-flex justify-content-center mt-3">
                        <button id="abort" class="btn btn-danger btn-sm" style="width: 140px; height:50px;">Abbrechen</button>
                    </div>
                </div>`;
            document.body.appendChild(modal);

            const emailInput = document.getElementById("emailPromptInput");

            document.getElementById('emailConfirm').onclick = () =>
            {
                const email = emailInput.value.trim();
                if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email))
                {
                    alert("Bitte gib eine gültige E-Mail-Adresse ein.");
                    return;
                }
                modal.remove();
                resolve({ continue: true, email: email || null });
            };

            document.getElementById('emailSkip').onclick = () =>
            {
                modal.remove();
                resolve({ continue: true, email: null });
            };

            document.getElementById('abort').onclick = () =>
            {
                modal.remove();
                resolve({ continue: false, email: null });
            };
        });
    }

    await fetchSession();
});