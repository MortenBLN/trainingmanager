window.addEventListener("DOMContentLoaded", async () =>
{
    const apiUrl = "/api/getSessions";
    const urlParams = new URLSearchParams(window.location.search);
    const sessionIdToSelect = urlParams.get("sessionId");

    const usernamesInput = document.getElementById("usernames");
    const checkboxContainer = document.getElementById("checkboxContainer");
    const numberInput = document.getElementById("numberInput");

    function updateCheckboxListFromTextarea()
    {
        const names = usernamesInput.value
            .split(",")
            .map(name => name.trim())
            .filter(name => name.length > 0);

        checkboxContainer.innerHTML = ""; // Clear old checkboxes

        names.forEach(name =>
        {
            const label = document.createElement("label");
            label.className = "d-block";

            const checkbox = document.createElement("input");
            checkbox.type = "checkbox";
            checkbox.value = name;
            checkbox.classList.add("user-checkbox", "mr-2");

            label.appendChild(checkbox);
            label.appendChild(document.createTextNode(name));
            checkboxContainer.appendChild(label);
        });

        limitCheckboxSelection(); // Re-apply limit
    }

    function limitCheckboxSelection()
    {
        const numberInput = document.getElementById("numberInput");
        const checkboxContainer = document.getElementById("checkboxContainer");

        function updateCheckboxStates()
        {
            const max = parseInt(numberInput.value, 10);
            const checkboxes = checkboxContainer.querySelectorAll("input[type='checkbox']");
            const checked = checkboxContainer.querySelectorAll("input[type='checkbox']:checked");

            checkboxes.forEach(checkbox =>
            {
                if (!checkbox.checked)
                {
                    checkbox.disabled = checked.length >= max;
                } else
                {
                    checkbox.disabled = false; // Always allow unchecking checked boxes
                }
            });
        }

        // Run on any checkbox change
        checkboxContainer.addEventListener("change", () =>
        {
            updateCheckboxStates();
        });

        // When numberInput changes, uncheck all and reset checkboxes
        numberInput.addEventListener("input", () =>
        {
            const checkboxes = checkboxContainer.querySelectorAll("input[type='checkbox']");
            checkboxes.forEach(cb =>
            {
                cb.checked = false;
                cb.disabled = false;
            });
            updateCheckboxStates();
        });
    }

    usernamesInput.addEventListener("input", updateCheckboxListFromTextarea);
    numberInput.addEventListener("input", limitCheckboxSelection);

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

                const dateStart = new Date(option.trainingStart);
                const formattedStart = dateStart.toLocaleString("de-DE", {
                    day: "numeric", month: "numeric", year: "numeric"
                });

                opt.textContent = option.teamname + " - " + formattedStart;
                opt.value = option.id;
                if (sessionIdToSelect && option.id === sessionIdToSelect)
                {
                    opt.selected = true;
                }
                dropdown.appendChild(opt);
            });

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

    document.getElementById("apiDropdown").addEventListener("change", async (event) =>
    {
        const sessionId = event.target.value;
        const res = await fetch(`/api/getSessionById/${sessionId}`);
        const session = await res.json();

        const validSubs = session.subscriptions.filter(sub => sub.subscriptionType !== 2);
        const usernamesString = validSubs.map(sub => sub.userName).join(", ");

        usernamesInput.value = usernamesString;
        updateCheckboxListFromTextarea();
    });

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

        // Build full list of usernames from textarea
        const nameList = usernames
            .split(",")
            .map(name => name.trim())
            .filter(Boolean);

        if (nameList.length === 0)
        {
            feedback.textContent = "Please enter at least one username.";
            return;
        }

        // Get checked usernames from checkbox list
        const checkedNames = Array.from(document.querySelectorAll('#checkboxContainer input[type="checkbox"]:checked'))
            .map(cb => cb.value.trim())
            .filter(Boolean);

        // Remove checked names from the general list to avoid duplicates
        const remainingNames = nameList.filter(name => !checkedNames.includes(name));

        // Shuffle function
        function shuffle(arr)
        {
            for (let i = arr.length - 1; i > 0; i--)
            {
                const j = Math.floor(Math.random() * (i + 1));
                [arr[i], arr[j]] = [arr[j], arr[i]];
            }
        }

        // Shuffle both arrays
        shuffle(checkedNames);
        shuffle(remainingNames);

        // Prepare empty groups
        const groups = Array.from({ length: number }, () => []);

        // Place one checked name per team (up to number of teams)
        checkedNames.forEach((name, i) =>
        {
            groups[i % number].push(name);
        });

        // Assign remaining names round-robin
        let teamIndex = 0;
        remainingNames.forEach(name =>
        {
            groups[teamIndex].push(name);
            teamIndex = (teamIndex + 1) % number;
        });

        // Display teams
        groups.forEach((group, idx) =>
        {
            const col = document.createElement("div");
            col.className = "col-md-4 mb-3";

            const card = document.createElement("div");
            card.className = "card shadow-sm";

            const cardBody = document.createElement("div");
            cardBody.className = "card-body";

            const title = document.createElement("h5");
            title.className = "card-title d-flex align-items-center";

            const mainTitle = document.createElement("span");
            mainTitle.textContent = `Team ${idx + 1}`;

            const subtitle = document.createElement("small");
            subtitle.className = "text-muted ml-2 small";
            subtitle.textContent = `- ${group.length}er Team  -`;

            title.appendChild(mainTitle);
            title.appendChild(subtitle);

            const ul = document.createElement("ul");
            ul.className = "list-group list-group-flush";
            group.forEach(name =>
            {
                const li = document.createElement("li");
                li.className = "list-group-item";

                // If this name is in checkedNames, make it bold
                if (checkedNames.includes(name))
                {
                    li.innerHTML = `<strong>${name}</strong>`;
                } else
                {
                    li.textContent = name;
                }

                ul.appendChild(li);
            });

            cardBody.appendChild(title);
            card.appendChild(cardBody);
            card.appendChild(ul);
            col.appendChild(card);

            listsContainer.appendChild(col);
        });

        feedback.textContent = `${groups.length} Teams mit ${nameList.length} Namen erstellt`;
    });

    loadDropdownOptions();
});