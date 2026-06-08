using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RealtimeQuiz.Data;
using RealtimeQuiz.Models;

namespace RealtimeQuiz.Pages;

public class QuizzesModel : PageModel
{
    private readonly QuizDbContext _db;

    public QuizzesModel(QuizDbContext db)
    {
        _db = db;
    }

    public string UserName { get; set; } = string.Empty;
    public List<QuizCard> Quizzes { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null)
        {
            return RedirectToPage("/StudentLogin");
        }

        UserName = HttpContext.Session.GetString("UserName") ?? "Student";
        Quizzes = await _db.QuizSessions
            .Where(quiz => quiz.IsPublished && !quiz.IsFinished)
            .Include(quiz => quiz.Participants)
            .OrderByDescending(quiz => quiz.PublishedAtUtc)
            .Select(quiz => new QuizCard(
                quiz.Title,
                quiz.Description,
                quiz.Code,
                quiz.Questions.Count,
                quiz.TotalMinutes,
                quiz.Participants.Any(participant => participant.AppUserId == userId.Value && participant.IsSubmitted)
                    ? "View Result"
                    : quiz.Participants.Any(participant => participant.AppUserId == userId.Value && participant.IsStarted)
                        ? "Continue"
                        : "Start Quiz"))
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostStart(string code)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null)
        {
            return RedirectToPage("/StudentLogin");
        }

        var session = await _db.QuizSessions.FirstOrDefaultAsync(quiz =>
            quiz.Code == code.ToUpper() && quiz.IsPublished && !quiz.IsFinished);
        var user = await _db.AppUsers.FindAsync(userId.Value);

        if (session is null || user is null)
        {
            return RedirectToPage("/Quizzes");
        }

        var participant = await _db.Participants.FirstOrDefaultAsync(participant =>
            participant.AppUserId == user.Id && participant.QuizSessionId == session.Id);

        if (participant is null)
        {
            participant = new Participant
            {
                AppUserId = user.Id,
                QuizSessionId = session.Id,
                Name = user.FullName,
                IsStarted = true,
                StartedAtUtc = DateTime.UtcNow,
                EndsAtUtc = DateTime.UtcNow.AddMinutes(session.TotalMinutes)
            };
            _db.Participants.Add(participant);
            await _db.SaveChangesAsync();
        }
        else if (!participant.IsStarted && !participant.IsSubmitted)
        {
            participant.IsStarted = true;
            participant.StartedAtUtc = DateTime.UtcNow;
            participant.EndsAtUtc = DateTime.UtcNow.AddMinutes(session.TotalMinutes);
            await _db.SaveChangesAsync();
        }

        return RedirectToPage("/Play", new { code = session.Code, participantId = participant.Id });
    }

    public record QuizCard(string Title, string Description, string Code, int QuestionCount, int TotalMinutes, string ActionText);
}
