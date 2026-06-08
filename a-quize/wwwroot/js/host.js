const room = document.querySelector("[data-code]");
const code = room.dataset.code;
const statusBox = document.querySelector("#hostStatus");
const leaderboard = document.querySelector("#leaderboard");

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/quizHub")
    .withAutomaticReconnect()
    .build();

connection.on("ParticipantJoined", name => {
    statusBox.textContent = `${name} opened the quiz.`;
});

connection.on("QuizFinished", () => {
    statusBox.textContent = "Quiz finished. Final leaderboard is ready.";
});

connection.on("LeaderboardUpdated", rows => {
    renderLeaderboard(rows);
});

connection.on("QuizError", message => {
    statusBox.textContent = message;
});

function renderLeaderboard(rows) {
    leaderboard.innerHTML = "";

    if (!rows.length) {
        leaderboard.innerHTML = "<li>No participants yet.</li>";
        return;
    }

    rows.forEach(row => {
        const item = document.createElement("li");
        item.innerHTML = `<span>${escapeHtml(row.name)}</span><strong>${row.score}</strong>`;
        leaderboard.appendChild(item);
    });
}

function escapeHtml(value) {
    const element = document.createElement("div");
    element.textContent = value;
    return element.innerHTML;
}

connection.start().then(() => {
    connection.invoke("HostJoin", code);
});
