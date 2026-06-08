using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RealtimeQuiz.Data;

namespace RealtimeQuiz.Pages;

public class LeaderboardModel : PageModel
{
    private readonly QuizDbContext _db;

    public LeaderboardModel(QuizDbContext db)
    {
        _db = db;
    }

    public string Code { get; set; } = string.Empty;
    public string SessionTitle { get; set; } = string.Empty;

    public async Task<IActionResult> OnGet(string code)
    {
        var session = await _db.QuizSessions.FirstOrDefaultAsync(quiz => quiz.Code == code.ToUpper());
        if (session is null)
        {
            return RedirectToPage("/Join");
        }

        Code = session.Code;
        SessionTitle = session.Title;
        return Page();
    }
}
