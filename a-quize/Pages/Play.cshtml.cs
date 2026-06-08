using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealtimeQuiz.Data;
using RealtimeQuiz.Hubs;
using RealtimeQuiz.Models;

namespace RealtimeQuiz.Pages;

public class PlayModel : PageModel
{
    private readonly QuizDbContext _db;
    private readonly IHubContext<QuizHub> _hub;

    public PlayModel(QuizDbContext db, IHubContext<QuizHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    public string Code { get; set; } = string.Empty;
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string SessionTitle { get; set; } = string.Empty;
    public string QuestionsJson { get; set; } = "[]";
    public string EndsAtUtcIso { get; set; } = string.Empty;
    public bool IsSubmitted { get; set; }
    public int Score { get; set; }

    public async Task<IActionResult> OnGet(string code, int participantId)
    {
        var loaded = await LoadAttempt(code, participantId);
        return loaded ?? Page();
    }

    public async Task<IActionResult> OnPost(string code, int participantId, string answersJson)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null)
        {
            return RedirectToPage("/StudentLogin");
        }

        var participant = await _db.Participants
            .Include(p => p.QuizSession)
            .ThenInclude(quiz => quiz!.Questions)
            .FirstOrDefaultAsync(p => p.Id == participantId
                && p.AppUserId == userId.Value
                && p.QuizSession!.Code == code.ToUpper());

        if (participant is null || participant.QuizSession is null)
        {
            return RedirectToPage("/Quizzes");
        }

        if (!participant.IsSubmitted)
        {
            var submittedAnswers = JsonSerializer.Deserialize<List<SubmittedAnswer>>(
                answersJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
            var answerLookup = submittedAnswers
                .GroupBy(answer => answer.QuestionId)
                .ToDictionary(group => group.Key, group => group.Last().SelectedOptionIndex);

            var questionIds = participant.QuizSession.Questions.Select(question => question.Id).ToList();
            var oldAnswers = _db.ParticipantAnswers.Where(answer =>
                answer.ParticipantId == participant.Id && questionIds.Contains(answer.QuestionId));
            _db.ParticipantAnswers.RemoveRange(oldAnswers);

            var score = 0;
            foreach (var question in participant.QuizSession.Questions.OrderBy(question => question.Order))
            {
                var selected = answerLookup.TryGetValue(question.Id, out var selectedIndex)
                    ? selectedIndex
                    : -1;
                var isCorrect = selected == question.CorrectOptionIndex;
                var points = isCorrect ? question.Points : 0;
                score += points;

                _db.ParticipantAnswers.Add(new ParticipantAnswer
                {
                    ParticipantId = participant.Id,
                    QuestionId = question.Id,
                    SelectedOptionIndex = selected,
                    IsCorrect = isCorrect,
                    PointsAwarded = points,
                    SubmittedAtUtc = DateTime.UtcNow
                });
            }

            participant.Score = score;
            participant.IsSubmitted = true;
            participant.SubmittedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await SendLeaderboard(participant.QuizSession);
        }

        return RedirectToPage("/Play", new { code = participant.QuizSession.Code, participantId = participant.Id });
    }

    private async Task<IActionResult?> LoadAttempt(string code, int participantId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null)
        {
            return RedirectToPage("/StudentLogin");
        }

        var participant = await _db.Participants
            .Include(p => p.QuizSession)
            .ThenInclude(quiz => quiz!.Questions)
            .FirstOrDefaultAsync(p => p.Id == participantId
                && p.AppUserId == userId.Value
                && p.QuizSession!.Code == code.ToUpper()
                && p.QuizSession.IsPublished);

        if (participant is null || participant.QuizSession is null)
        {
            return RedirectToPage("/Quizzes");
        }

        if (!participant.IsSubmitted && (!participant.IsStarted || participant.EndsAtUtc is null))
        {
            participant.IsStarted = true;
            participant.StartedAtUtc = DateTime.UtcNow;
            participant.EndsAtUtc = DateTime.UtcNow.AddMinutes(participant.QuizSession.TotalMinutes);
            await _db.SaveChangesAsync();
        }

        Code = participant.QuizSession.Code;
        ParticipantId = participant.Id;
        ParticipantName = participant.Name;
        SessionTitle = participant.QuizSession.Title;
        IsSubmitted = participant.IsSubmitted;
        Score = participant.Score;
        var endsAtUtc = participant.EndsAtUtc ?? DateTime.UtcNow.AddMinutes(participant.QuizSession.TotalMinutes);
        EndsAtUtcIso = DateTime.SpecifyKind(endsAtUtc, DateTimeKind.Utc).ToString("O");
        QuestionsJson = JsonSerializer.Serialize(participant.QuizSession.Questions
            .OrderBy(question => question.Order)
            .Select(question => new QuestionData(
                question.Id,
                question.Order,
                question.Text,
                question.GetOptions())));

        return null;
    }

    private async Task SendLeaderboard(QuizSession session)
    {
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

        await _hub.Clients.Group(session.Code).SendAsync("LeaderboardUpdated", leaderboard);
    }

    public record QuestionData(int Id, int Order, string Text, string[] Options);
    public record SubmittedAnswer(int QuestionId, int SelectedOptionIndex);
}
