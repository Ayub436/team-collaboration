using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RealtimeQuiz.Pages;

public class LoginModel : PageModel
{
    private readonly IConfiguration _configuration;

    public LoginModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public string? Message { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        var configuredPassword = _configuration["Admin:Password"] ?? "admin123";

        if (Input.Password == configuredPassword)
        {
            HttpContext.Session.SetString("IsHost", "true");
            return RedirectToPage("/Host");
        }

        Message = "Invalid host password.";
        return Page();
    }

    public class LoginInput
    {
        public string Password { get; set; } = string.Empty;
    }
}
