namespace RealtimeQuiz.Models;

public class Participant
{
    public int Id { get; set; }
    public int AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    public int QuizSessionId { get; set; }
    public QuizSession? QuizSession { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool IsStarted { get; set; }
    public bool IsSubmitted { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? EndsAtUtc { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
}
