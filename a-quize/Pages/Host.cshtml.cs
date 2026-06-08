using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RealtimeQuiz.Data;
using RealtimeQuiz.Models;

namespace RealtimeQuiz.Pages;

public class HostModel : PageModel
{
    private readonly QuizDbContext _db;

    public HostModel(QuizDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public CreateQuizInput CreateQuiz { get; set; } = new();

    public List<QuizSummary> Quizzes { get; set; } = [];
    public string? Message { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (HttpContext.Session.GetString("IsHost") != "true")
        {
            return RedirectToPage("/Login");
        }

        await LoadQuizzes();
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (HttpContext.Session.GetString("IsHost") != "true")
        {
            return RedirectToPage("/Login");
        }

        if (string.IsNullOrWhiteSpace(CreateQuiz.Title))
        {
            Message = "Please enter a quiz title.";
            await LoadQuizzes();
            return Page();
        }

        var session = new QuizSession
        {
            Title = CreateQuiz.Title.Trim(),
            Description = CreateQuiz.Description.Trim(),
            PlannedQuestionCount = Math.Clamp(CreateQuiz.QuestionCount, 1, 50),
            TotalMinutes = Math.Clamp(CreateQuiz.TotalMinutes, 1, 180),
            Code = await GenerateCode()
        };

        _db.QuizSessions.Add(session);
        await _db.SaveChangesAsync();

        return RedirectToPage("/BuildQuiz", new { code = session.Code });
    }

    private async Task LoadQuizzes()
    {
        if (HttpContext.Session.GetString("IsHost") != "true")
        {
            return;
        }

        Quizzes = await _db.QuizSessions
            .OrderByDescending(quiz => quiz.CreatedAtUtc)
            .Take(8)
            .Select(quiz => new QuizSummary(
                quiz.Title,
                quiz.Code,
                quiz.Questions.Count,
                quiz.IsPublished ? "Published" : "Draft"))
            .ToListAsync();
    }

    private async Task<string> GenerateCode()
    {
        const string letters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();

        while (true)
        {
            var code = new string(Enumerable.Range(0, 6).Select(_ => letters[random.Next(letters.Length)]).ToArray());
            var exists = await _db.QuizSessions.AnyAsync(session => session.Code == code);
            if (!exists)
            {
                return code;
            }
        }
    }

    public class CreateQuizInput
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int QuestionCount { get; set; } = 5;
        public int TotalMinutes { get; set; } = 10;
    }

    public record QuizSummary(string Title, string Code, int QuestionCount, string Status);
}
