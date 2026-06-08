using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RealtimeQuiz.Data;
using RealtimeQuiz.Models;

namespace RealtimeQuiz.Pages;

public class RegisterModel : PageModel
{
    private readonly QuizDbContext _db;

    public RegisterModel(QuizDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    public string? Message { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        var email = Input.Email.Trim().ToLower();
        var name = Input.FullName.Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(Input.Password))
        {
            Message = "Enter your name, email, and password.";
            return Page();
        }

        var exists = await _db.AppUsers.AnyAsync(user => user.Email == email);
        if (exists)
        {
            Message = "That email is already registered.";
            return Page();
        }

        var user = new AppUser
        {
            FullName = name,
            Email = email,
            Password = Input.Password
        };

        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync();

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.FullName);

        return RedirectToPage("/Quizzes");
    }

    public class RegisterInput
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
