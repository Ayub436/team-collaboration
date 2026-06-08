using Microsoft.EntityFrameworkCore;
using RealtimeQuiz.Models;

namespace RealtimeQuiz.Data;

public class QuizDbContext : DbContext
{
    public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options)
    {
    }

    public DbSet<QuizSession> QuizSessions => Set<QuizSession>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<ParticipantAnswer> ParticipantAnswers => Set<ParticipantAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QuizSession>()
            .HasIndex(session => session.Code)
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<Participant>()
            .HasIndex(participant => new { participant.AppUserId, participant.QuizSessionId })
            .IsUnique();

        modelBuilder.Entity<Question>()
            .Property(question => question.OptionsJson)
            .HasColumnType("json");

        modelBuilder.Entity<ParticipantAnswer>()
            .HasIndex(answer => new { answer.ParticipantId, answer.QuestionId })
            .IsUnique();
    }
}
