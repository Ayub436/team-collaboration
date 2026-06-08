using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealtimeQuiz.Data;
using RealtimeQuiz.Hubs;
using RealtimeQuiz.Models;

namespace RealtimeQuiz.Pages;

public class BuildQuizModel : PageModel
{
    private readonly QuizDbContext _db;
    private readonly IHubContext<QuizHub> _hub;

    public BuildQuizModel(QuizDbContext db, IHubContext<QuizHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    [BindProperty]
    public QuestionForm QuestionInput { get; set; } = new();

    public string Code { get; set; } = string.Empty;
    public string SessionTitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PlannedQuestionCount { get; set; }
    public int AddedQuestionCount { get; set; }
    public bool IsPublished { get; set; }
    public bool CanAddMore => AddedQuestionCount < PlannedQuestionCount && !IsPublished;
    public string? Message { get; set; }
    public List<QuestionSummary> Questions { get; set; } = [];

    public async Task<IActionResult> OnGet(string code)
    {
        if (HttpContext.Session.GetString("IsHost") != "true")
        {
            return RedirectToPage("/Login");
        }

        return await LoadPage(code);
    }

    public async Task<IActionResult> OnPostAddQuestion(string code)
    {
        if (HttpContext.Session.GetString("IsHost") != "true")
        {
            return RedirectToPage("/Login");
        }

        var session = await _db.QuizSessions
            .Include(quiz => quiz.Questions)
            .FirstOrDefaultAsync(quiz => quiz.Code == code.ToUpper());

        if (session is null)
        {
            return RedirectToPage("/Host");
        }

        if (session.IsPublished)
        {
            Message = "This quiz has already been published.";
            return await LoadPage(code);
        }

        if (session.Questions.Count >= session.PlannedQuestionCount)
        {
            Message = "You have already added the planned number of questions.";
            return await LoadPage(code);
        }

        var options = new[] { QuestionInput.OptionA, QuestionInput.OptionB, QuestionInput.OptionC, QuestionInput.OptionD }
            .Select(option => option.Trim())
            .ToArray();

        if (string.IsNullOrWhiteSpace(QuestionInput.Text) || options.Any(string.IsNullOrWhiteSpace))
        {
            Message = "Enter the question and all four options.";
            return await LoadPage(code);
        }

        var question = new Question
        {
            QuizSessionId = session.Id,
            Order = session.Questions.Count + 1,
            Text = QuestionInput.Text.Trim(),
            CorrectOptionIndex = Math.Clamp(QuestionInput.CorrectOptionIndex, 0, 3),
            Seconds = Math.Max(10, (session.TotalMinutes * 60) / Math.Max(session.PlannedQuestionCount, 1)),
            Points = Math.Clamp(QuestionInput.Points, 1, 100)
        };
        question.SetOptions(options);

        _db.Questions.Add(question);
        await _db.SaveChangesAsync();

        return RedirectToPage("/BuildQuiz", new { code = session.Code });
    }

    public async Task<IActionResult> OnPostPublish(string code)
    {
        if (HttpContext.Session.GetString("IsHost") != "true")
        {
            return RedirectToPage("/Login");
        }

        var session = await _db.QuizSessions
            .Include(quiz => quiz.Questions)
            .FirstOrDefaultAsync(quiz => quiz.Code == code.ToUpper());

        if (session is null)
        {
            return RedirectToPage("/Host");
        }

        if (session.Questions.Count < session.PlannedQuestionCount)
        {
            Message = "Add all questions before publishing.";
            return await LoadPage(code);
        }

        session.IsPublished = true;
        session.PublishedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _hub.Clients.Group("published-quizzes").SendAsync("PublishedQuizzesChanged");

        return RedirectToPage("/HostRoom", new { code = session.Code });
    }

    private async Task<IActionResult> LoadPage(string code)
    {
        var session = await _db.QuizSessions
            .Include(quiz => quiz.Questions)
            .FirstOrDefaultAsync(quiz => quiz.Code == code.ToUpper());

        if (session is null)
        {
            return RedirectToPage("/Host");
        }

        Code = session.Code;
        SessionTitle = session.Title;
        Description = session.Description;
        PlannedQuestionCount = session.PlannedQuestionCount;
        AddedQuestionCount = session.Questions.Count;
        IsPublished = session.IsPublished;
        Questions = session.Questions
            .OrderBy(question => question.Order)
            .Select(question => new QuestionSummary(question.Order, question.Text, question.Points))
            .ToList();

        return Page();
    }

    public class QuestionForm
    {
        public string Text { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public int CorrectOptionIndex { get; set; }
        public int Points { get; set; } = 10;
    }

    public record QuestionSummary(int Order, string Text, int Points);
}
