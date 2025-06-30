window.addEventListener("DOMContentLoaded", async () =>
{
    const apiUrl = "/api/getSessions";
    const urlParams = new URLSearchParams(window.location.search);
    const sessionIdToSelect = urlParams.get("sessionId");

    async function loadDropdownOptions()
    {
        const dropdown = document.getElementById("apiDropdown");
        try
        {
            const response = await fetch(apiUrl);
            const data = await response.json();

            dropdown.innerHTML = '<option disabled selected>Session Auswahl</option>';
            data.sessions.forEach(option =>
            {
                const opt = document.createElement("option");

                // Add the start date
                const dateStart = new Date(option.trainingStart);
                const formattedStart = dateStart.toLocaleString("de-DE", {
                    day: "numeric",
                    month: "numeric",
                    year: "numeric",
                    hour: undefined,
                    minute: undefined,
                    second: undefined
                });

                opt.textContent = option.teamname + " - " + formattedStart;
                opt.value = option.id; // <-- sessionId
                if (sessionIdToSelect && option.id === sessionIdToSelect)
                {
                    opt.selected = true;
                }
                dropdown.appendChild(opt);
            });

            // Triggers the onchange event, when the Teamdrafter was openend from within a session
            if (sessionIdToSelect)
            {
                dropdown.dispatchEvent(new Event("change"));
            }
        } catch (error)
        {
            dropdown.innerHTML = '<option disabled selected>Error loading options</option>';
            console.error("Error fetching dropdown options:", error);
        }
    }

    document.getElementById("submitButton").addEventListener("click", () =>
    {
        const selectedOption = document.getElementById("apiDropdown").value;
        const usernames = document.getElementById("usernames").value.trim();
        const number = parseInt(document.getElementById("numberInput").value, 10);

        const feedback = document.getElementById("feedback");
        const listsContainer = document.getElementById("listsContainer");
        listsContainer.innerHTML = ""; // Clear previous lists

        if (!selectedOption || !usernames || isNaN(number) || number < 2 || number > 6)
        {
            feedback.textContent = "Please fill in all fields correctly.";
            return;
        }

        const nameList = usernames
            .split(",")
            .map(name => name.trim())
            .filter(Boolean);

        if (nameList.length === 0)
        {
            feedback.textContent = "Please enter at least one username.";
            return;
        }

        for (let i = nameList.length - 1; i > 0; i--)
        {
            const j = Math.floor(Math.random() * (i + 1));
            [nameList[i], nameList[j]] = [nameList[j], nameList[i]];
        }

        const totalUsers = nameList.length;
        const baseSize = Math.floor(totalUsers / number);
        const remainder = totalUsers % number;

        let currentIndex = 0;
        const groups = [];

        for (let i = 0; i < number; i++)
        {
            const extra = i < remainder ? 1 : 0;
            const groupSize = baseSize + extra;
            const group = nameList.slice(currentIndex, currentIndex + groupSize);
            groups.push(group);
            currentIndex += groupSize;
        }

        groups.forEach((group, idx) =>
        {
            const col = document.createElement("div");
            col.className = "col-md-4 mb-3";

            const card = document.createElement("div");
            card.className = "card shadow-sm";

            const cardBody = document.createElement("div");
            cardBody.className = "card-body";

            const title = document.createElement("h5");
            title.className = "card-title";
            title.textContent = `Team ${idx + 1}`;

            const ul = document.createElement("ul");
            ul.className = "list-group list-group-flush";
            group.forEach(name =>
            {
                const li = document.createElement("li");
                li.className = "list-group-item";
                li.textContent = name;
                ul.appendChild(li);
            });

            cardBody.appendChild(title);
            card.appendChild(cardBody);
            card.appendChild(ul);
            col.appendChild(card);

            listsContainer.appendChild(col);
        });

        feedback.textContent = "Lists generated!";
    });

    document.getElementById("apiDropdown").addEventListener("change", async (event) =>
    {
        const sessionId = event.target.value; // Now contains sessionId
        var usernamesList = document.getElementById("usernames");

        const res = await fetch(`/api/getSessionById/${sessionId}`);
        const session = await res.json();

        const validSubs = session.subscriptions
            .filter(sub => sub.subscriptionType !== 2);

        const usernamesString = validSubs
            .map(sub => sub.userName)
            .join(", ");

        usernamesList.value = usernamesString;

       
    });

    loadDropdownOptions();
});