const connection = new signalR.HubConnectionBuilder()
    .withUrl("/quizHub")
    .withAutomaticReconnect()
    .build();

connection.on("PublishedQuizzesChanged", () => {
    window.location.reload();
});

connection.start().then(() => {
    connection.invoke("WatchPublishedQuizzes");
});
