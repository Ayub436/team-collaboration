using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RealtimeQuiz.Data;
using RealtimeQuiz.Models;

namespace RealtimeQuiz.Pages;

public class JoinModel : PageModel
{
    private readonly QuizDbContext _db;

    public JoinModel(QuizDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public JoinInput Input { get; set; } = new();

    public string? Message { get; set; }

    public IActionResult OnGet()
    {
        return RedirectToPage("/StudentLogin");
    }

    public IActionResult OnPost()
    {
        return RedirectToPage("/StudentLogin");
    }

    public class JoinInput
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
