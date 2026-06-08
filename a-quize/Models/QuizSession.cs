namespace RealtimeQuiz.Models;

public class QuizSession
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int PlannedQuestionCount { get; set; }
    public int TotalMinutes { get; set; } = 10;
    public int CurrentQuestionIndex { get; set; } = -1;
    public bool IsPublished { get; set; }
    public bool IsStarted { get; set; }
    public bool IsFinished { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? QuizEndsAtUtc { get; set; }
    public DateTime? QuestionEndsAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<Question> Questions { get; set; } = [];
    public List<Participant> Participants { get; set; } = [];
}
