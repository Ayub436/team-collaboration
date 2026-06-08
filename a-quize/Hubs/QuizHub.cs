using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealtimeQuiz.Data;

namespace RealtimeQuiz.Hubs;

public class QuizHub : Hub
{
    private readonly QuizDbContext _db;

    public QuizHub(QuizDbContext db)
    {
        _db = db;
    }

    public async Task WatchPublishedQuizzes()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "published-quizzes");
    }

    public async Task JoinSession(string code, int participantId)
    {
        var session = await FindSession(code);
        if (session is null || !session.IsPublished)
        {
            await Clients.Caller.SendAsync("QuizError", "Quiz session was not found.");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, session.Code);

        var participant = await _db.Participants.FindAsync(participantId);
        if (participant is not null)
        {
            await Clients.Group(session.Code).SendAsync("ParticipantJoined", participant.Name);
        }

        await SendLeaderboard(session.Code);
    }

    public async Task HostJoin(string code)
    {
        var session = await FindSession(code);
        if (session is null || !session.IsPublished)
        {
            await Clients.Caller.SendAsync("QuizError", "Quiz session was not found.");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, session.Code);
        await SendLeaderboard(session.Code);
    }

    public async Task SendLeaderboard(string code)
    {
        var session = await FindSession(code);
        if (session is null)
        {
            return;
        }

        var leaderboard = await _db.Participants
            .Where(participant => participant.QuizSessionId == session.Id && participant.IsSubmitted)
            .OrderByDescending(participant => participant.Score)
            .ThenBy(participant => participant.SubmittedAtUtc)
            .Select(participant => new
            {
                participant.Name,
                participant.Score
            })
            .ToListAsync();

        await Clients.Group(session.Code).SendAsync("LeaderboardUpdated", leaderboard);
    }

    private Task<Models.QuizSession?> FindSession(string code) =>
        _db.QuizSessions.FirstOrDefaultAsync(session => session.Code == code.ToUpper());

}
