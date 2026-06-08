using System.Text.Json;

namespace RealtimeQuiz.Models;

public class Question
{
    public int Id { get; set; }
    public int QuizSessionId { get; set; }
    public QuizSession? QuizSession { get; set; }
    public int Order { get; set; }
    public string Text { get; set; } = string.Empty;
    public string OptionsJson { get; set; } = "[]";
    public int CorrectOptionIndex { get; set; }
    public int Seconds { get; set; } = 20;
    public int Points { get; set; } = 10;

    public string[] GetOptions() =>
        JsonSerializer.Deserialize<string[]>(OptionsJson) ?? [];

    public void SetOptions(params string[] options) =>
        OptionsJson = JsonSerializer.Serialize(options);
}
