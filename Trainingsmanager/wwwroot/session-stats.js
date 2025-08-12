let allSessions = []; // Global store
let chart; // Global chart reference

document.addEventListener("DOMContentLoaded", async () =>
{
    await loadDropdownOptions();
    await fetchSessions();

    document.getElementById("statsDropdown").addEventListener("change", updateStatistics);
    document.getElementById("timeSpanDropdown").addEventListener("change", updateStatistics);
});

async function fetchSessions()
{
    try
    {
        const response = await fetch("/api/getSessions", { cache: "no-store" });
        const data = await response.json();
        allSessions = data.sessions;

        console.log("Sessions loaded:", allSessions);
    } catch (error)
    {
        console.error("Failed to fetch sessions:", error);
    }
}

async function loadDropdownOptions()
{
    const dropdown = document.getElementById("statsDropdown");
    const options = ["Anzahl Teilnahmen", "Teilnahme pro Spieler*in"];

    options.forEach(option =>
    {
        const opt = document.createElement("option");

        opt.textContent = option;
        opt.value = option;
        dropdown.appendChild(opt);

    });   
}

function updateStatistics()
{
    const selectedStat = document.getElementById("statsDropdown").value;
    const timeSpan = parseInt(document.getElementById("timeSpanDropdown").value);

    if (selectedStat === "Anzahl Teilnahmen")
    {
        renderParticipationChart(timeSpan);
    }
    else if (selectedStat === "Teilnahme pro Spieler*in")
    {
        renderPlayerAttendanceChart(timeSpan);
    }
}

function renderParticipationChart(monthSpan)
{
    const now = new Date();
    const cutoffDate = new Date();
    cutoffDate.setMonth(now.getMonth() - monthSpan);

    const filtered = allSessions
        .filter(s =>
        {
            const date = new Date(s.trainingStart);
            return date >= cutoffDate && date <= now;
        })
        .sort((a, b) => new Date(a.trainingStart) - new Date(b.trainingStart)); // sort ascending

    const labels = filtered.map(s => new Date(s.trainingStart).toLocaleDateString());
    const data = filtered.map(s => s.subscriptions.length);

    // 🔢 Calculate average
    const averageRaw = data.length ? data.reduce((sum, val) => sum + val, 0) / data.length : 0;
    const average = Number.isInteger(averageRaw) ? averageRaw : averageRaw.toFixed(2);

    // 📝 Show average in the UI
    const avgDisplay = document.getElementById("average-display");
    avgDisplay.textContent = `Durchschnitt: ${average} Teilnahmen`;

    // 📊 Draw chart
    const ctx = getChartCanvas();

    if (chart)
    {
        chart.destroy();
    }

    chart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels,
            datasets: [{
                label: 'Teilnahmen pro Session',
                data,
                backgroundColor: 'rgba(54, 162, 235, 0.6)',
                borderColor: 'rgba(54, 162, 235, 1)',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: { beginAtZero: true },
                x: { title: { display: true, text: 'Datum' } }
            }
        }
    });
}


function renderPlayerAttendanceChart(monthSpan)
{
    const now = new Date();
    const cutoffDate = new Date();
    cutoffDate.setMonth(now.getMonth() - monthSpan);

    const filtered = allSessions.filter(s =>
    {
        const date = new Date(s.trainingStart);
        return date >= cutoffDate && date <= now;
    });

    // Map to count attendance per player name
    const attendanceMap = new Map();

    filtered.forEach(session =>
    {
        session.subscriptions.forEach(sub =>
        {
            const name = sub.userName;

            if (!name) return;

            attendanceMap.set(name, (attendanceMap.get(name) || 0) + 1);
        });
    });

    // Sort by attendance count descending, then by name ascending
    const sortedEntries = Array.from(attendanceMap.entries()).sort((a, b) =>
    {
        const diff = b[1] - a[1];
        if (diff !== 0) return diff;
        return a[0].localeCompare(b[0]);
    });

    const labels = sortedEntries.map(entry => entry[0]);
    const data = sortedEntries.map(entry => entry[1]);

    // Calculate average attendance per player
    const totalAttendances = data.reduce((sum, val) => sum + val, 0);
    const averageRaw = data.length ? totalAttendances / data.length : 0;
    const average = Number.isInteger(averageRaw) ? averageRaw : averageRaw.toFixed(2);

    // Show average above the chart
    const avgDisplay = document.getElementById("average-display");
    avgDisplay.textContent = `Durchschnitt: ${average} Teilnahmen pro Spieler*in`;

    // Draw chart
    const canvas = getChartCanvas();

    // 🧠 Only adjust height on mobile screens (<=768px)
    if (window.innerWidth <= 768)
    {
        const heightPerLabel = 40; // Adjust if needed
        canvas.height = labels.length * heightPerLabel;
    } else
    {
        canvas.height = null; // Reset on larger screens
    }

    const ctx = canvas.getContext('2d');
    if (chart)
    {
        chart.destroy();
    }

    chart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels,
            datasets: [{
                label: 'Teilnahmen pro Spieler*in',
                data,
                backgroundColor: 'rgba(255, 159, 64, 0.6)',
                borderColor: 'rgba(255, 159, 64, 1)',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            indexAxis: labels.length > 10 ? 'y' : 'x', // horizontal chart if too many players
            scales: {
                y: {
                    beginAtZero: true,
                    stepSize: 1,
                },
                x: { title: { display: true, text: 'Anzahl Teilnahmen' } }
            }
        }
    });
}


function getChartCanvas()
{
    let canvas = document.getElementById("statsChart");
    if (!canvas)
    {
        canvas = document.createElement("canvas");
        canvas.id = "statsChart";
        canvas.className = "mt-4";
        document.getElementById("redirect-button-container").appendChild(canvas);
    }
    return canvas;
}

