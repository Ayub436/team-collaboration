using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RealtimeQuiz.Data;

namespace RealtimeQuiz.Pages;

public class StudentLoginModel : PageModel
{
    private readonly QuizDbContext _db;

    public StudentLoginModel(QuizDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public string? Message { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        var email = Input.Email.Trim().ToLower();
        var user = await _db.AppUsers.FirstOrDefaultAsync(user =>
            user.Email == email && user.Password == Input.Password);

        if (user is null)
        {
            Message = "Invalid email or password.";
            return Page();
        }

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.FullName);

        return RedirectToPage("/Quizzes");
    }

    public class LoginInput
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
