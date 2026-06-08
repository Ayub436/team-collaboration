namespace RealtimeQuiz.Models;

public class ParticipantAnswer
{
    public int Id { get; set; }
    public int ParticipantId { get; set; }
    public Participant? Participant { get; set; }
    public int QuestionId { get; set; }
    public Question? Question { get; set; }
    public int SelectedOptionIndex { get; set; }
    public bool IsCorrect { get; set; }
    public int PointsAwarded { get; set; }
    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
}
