const room = document.querySelector("[data-code]");
const code = room.dataset.code;
const leaderboard = document.querySelector("#leaderboard");

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/quizHub")
    .withAutomaticReconnect()
    .build();

connection.on("LeaderboardUpdated", rows => {
    leaderboard.innerHTML = "";

    if (!rows.length) {
        leaderboard.innerHTML = "<li>No scores yet.</li>";
        return;
    }

    rows.forEach(row => {
        const item = document.createElement("li");
        item.innerHTML = `<span>${escapeHtml(row.name)}</span><strong>${row.score}</strong>`;
        leaderboard.appendChild(item);
    });
});

function escapeHtml(value) {
    const element = document.createElement("div");
    element.textContent = value;
    return element.innerHTML;
}

connection.start().then(() => {
    connection.invoke("HostJoin", code);
});
