using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Models.Enums;

namespace Tests_and_Interviews.Services
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<TestAttempt> TestAttempts { get; set; }
        public DbSet<InterviewSession> InterviewSessions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=WinUIDevDb;Username=devuser;Password=devpassword");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.TestAttempt)
                .WithMany(ta => ta.Answers)
                .HasForeignKey(a => a.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Test)
                .WithMany(t => t.Questions)
                .HasForeignKey(q => q.TestId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TestAttempt>()
                .HasOne(ta => ta.Test)
                .WithMany(t => t.Attempts)
                .HasForeignKey(ta => ta.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TestAttempt>()
                .HasOne(ta => ta.User)
                .WithMany()
                .HasForeignKey(ta => ta.ExternalUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TestAttempt>()
                .Property(ta => ta.Id)
                .UseIdentityAlwaysColumn();
        }

        public void SeedDatabase()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();

            if (!Users.Any())
            {
                Users.AddRange(
                    new User { Id = 1, Name = "Alice Johnson", Email = "alice@example.com" },
                    new User { Id = 2, Name = "Bob Smith", Email = "bob@example.com" },
                    new User { Id = 3, Name = "Carol Williams", Email = "carol@example.com" }
                );
                SaveChanges();
            }

            if (!Tests.Any())
            {
                Tests.AddRange(
                    new Test
                    {
                        Id = 1,
                        Title = "C# Fundamentals",
                        Category = "Programming",
                        CreatedAt = new DateTime(2025, 1, 10, 9, 0, 0, DateTimeKind.Utc)
                    },
                    new Test
                    {
                        Id = 2,
                        Title = "SQL Basics",
                        Category = "Database",
                        CreatedAt = new DateTime(2025, 2, 5, 10, 0, 0, DateTimeKind.Utc)
                    }
                );
                SaveChanges();
            }

            if (!Questions.Any())
            {
                Questions.AddRange(
                    new Question
                    {
                        PositionId = 1,
                        TestId = null,
                        QuestionText = "Tell us about your favourite project.",
                        QuestionTypeString = QuestionType.INTERVIEW.ToString(),
                        QuestionScore = 10f,
                        QuestionAnswer = null
                    },
                    new Question
                    {
                        PositionId = 1,
                        TestId = null,
                        QuestionText = "How do you handle conflict in a team?",
                        QuestionTypeString = QuestionType.INTERVIEW.ToString(),
                        QuestionScore = 10f,
                        QuestionAnswer = null
                    },
                    new Question
                    {
                        PositionId = 2,
                        TestId = null,
                        QuestionText = "Where do you see yourself in 5 years?",
                        QuestionTypeString = QuestionType.INTERVIEW.ToString(),
                        QuestionScore = 10f,
                        QuestionAnswer = null
                    },
                    new Question
                    {
                        PositionId = null,
                        TestId = 1,
                        QuestionText = "C# is a statically typed language.",
                        QuestionTypeString = QuestionType.TRUE_FALSE.ToString(),
                        QuestionScore = 5f,
                        QuestionAnswer = "true"
                    },
                    new Question
                    {
                        PositionId = null,
                        TestId = 1,
                        QuestionText = "In C#, a string is a value type.",
                        QuestionTypeString = QuestionType.TRUE_FALSE.ToString(),
                        QuestionScore = 5f,
                        QuestionAnswer = "false"
                    },
                    new Question
                    {
                        PositionId = null,
                        TestId = 1,
                        QuestionText = "Which keyword is used to define an interface in C#?",
                        QuestionTypeString = QuestionType.SINGLE_CHOICE.ToString(),
                        QuestionScore = 10f,
                        QuestionAnswer = "1"
                    },
                    new Question
                    {
                        PositionId = null,
                        TestId = 1,
                        QuestionText = "Which of the following are C# access modifiers?",
                        QuestionTypeString = QuestionType.MULTIPLE_CHOICE.ToString(),
                        QuestionScore = 10f,
                        QuestionAnswer = "[0,1,2]"
                    },
                    new Question
                    {
                        PositionId = null,
                        TestId = 2,
                        QuestionText = "What SQL keyword is used to retrieve data from a table?",
                        QuestionTypeString = QuestionType.TEXT.ToString(),
                        QuestionScore = 10f,
                        QuestionAnswer = "SELECT"
                    },
                    new Question
                    {
                        PositionId = null,
                        TestId = 2,
                        QuestionText = "Which SQL clause filters rows after grouping?",
                        QuestionTypeString = QuestionType.TEXT.ToString(),
                        QuestionScore = 10f,
                        QuestionAnswer = "HAVING"
                    },
                    new Question
                    {
                        PositionId = null,
                        TestId = 2,
                        QuestionText = "Which JOIN returns all rows from the left table?",
                        QuestionTypeString = QuestionType.SINGLE_CHOICE.ToString(),
                        QuestionScore = 10f,
                        QuestionAnswer = "2"
                    }
                );
                SaveChanges();
            }

            if (!TestAttempts.Any())
            {
                TestAttempts.AddRange(
                    new TestAttempt
                    {
                        TestId = 1,
                        ExternalUserId = 1,
                        Score = 25m,
                        Status = TestStatus.SUBMITTED.ToString(),
                        StartedAt = new DateTime(2025, 3, 1, 10, 0, 0, DateTimeKind.Utc),
                        CompletedAt = new DateTime(2025, 3, 1, 10, 45, 0, DateTimeKind.Utc),
                        AnswersFilePath = "answers/attempt_1.json"
                    },
                    new TestAttempt
                    {
                        TestId = 2,
                        ExternalUserId = 2,
                        Score = 18m,
                        Status = TestStatus.REVIEWED.ToString(),
                        StartedAt = new DateTime(2025, 3, 5, 14, 0, 0, DateTimeKind.Utc),
                        CompletedAt = new DateTime(2025, 3, 5, 14, 30, 0, DateTimeKind.Utc),
                        AnswersFilePath = "answers/attempt_2.json"
                    },
                    new TestAttempt
                    {
                        TestId = 1,
                        ExternalUserId = 3,
                        Score = 0m,
                        Status = TestStatus.NOT_STARTED.ToString(),
                        StartedAt = new DateTime(2025, 3, 10, 9, 0, 0, DateTimeKind.Utc),
                        CompletedAt = null,
                        AnswersFilePath = ""
                    }
                );
                SaveChanges();
            }

            if (!Answers.Any())
            {
                var attempt1 = TestAttempts.First(ta => ta.ExternalUserId == 1 && ta.TestId == 1);
                var attempt2 = TestAttempts.First(ta => ta.ExternalUserId == 2 && ta.TestId == 2);

                var q_tf1 = Questions.First(q => q.QuestionText == "C# is a statically typed language.");
                var q_tf2 = Questions.First(q => q.QuestionText == "In C#, a string is a value type.");
                var q_sc1 = Questions.First(q => q.QuestionText == "Which keyword is used to define an interface in C#?");
                var q_mc1 = Questions.First(q => q.QuestionText == "Which of the following are C# access modifiers?");

                Answers.AddRange(
                    new Answer { AttemptId = attempt1.Id, QuestionId = q_tf1.Id, Value = "true" },
                    new Answer { AttemptId = attempt1.Id, QuestionId = q_tf2.Id, Value = "false" },
                    new Answer { AttemptId = attempt1.Id, QuestionId = q_sc1.Id, Value = "1" },
                    new Answer { AttemptId = attempt1.Id, QuestionId = q_mc1.Id, Value = "[0,1,2]" }
                );

                var q_txt1 = Questions.First(q => q.QuestionText == "What SQL keyword is used to retrieve data from a table?");
                var q_txt2 = Questions.First(q => q.QuestionText == "Which SQL clause filters rows after grouping?");
                var q_sc2 = Questions.First(q => q.QuestionText == "Which JOIN returns all rows from the left table?");

                Answers.AddRange(
                    new Answer { AttemptId = attempt2.Id, QuestionId = q_txt1.Id, Value = "SELECT" },
                    new Answer { AttemptId = attempt2.Id, QuestionId = q_txt2.Id, Value = "WHERE" },
                    new Answer { AttemptId = attempt2.Id, QuestionId = q_sc2.Id, Value = "2" }
                );

                SaveChanges();
            }

            if (!InterviewSessions.Any())
            {
                InterviewSessions.AddRange(
                    new InterviewSession
                    {
                        PositionId = 1,
                        ExternalUserId = 1,
                        InterviewerId = 2,
                        DateStart = new DateTime(2025, 4, 1, 10, 0, 0, DateTimeKind.Utc),
                        Video = "recordings/session_1.mp4",
                        Status = InterviewStatus.Completed.ToString(),
                        Score = 8.5m
                    },
                    new InterviewSession
                    {
                        PositionId = 2,
                        ExternalUserId = 3,
                        InterviewerId = 2,
                        DateStart = new DateTime(2025, 4, 15, 14, 0, 0, DateTimeKind.Utc),
                        Video = "",
                        Status = InterviewStatus.Scheduled.ToString(),
                        Score = 0m
                    }
                );
                SaveChanges();
            }
        }

        public async Task<List<Question>> GetInterviewQuestionsByPositionAsync(int positionId)
        {
            return await Questions
                .Where(q => q.QuestionTypeString == QuestionType.INTERVIEW.ToString()
                         && q.PositionId == positionId)
                .ToListAsync();
        }

        public async Task<InterviewSession> GetInterviewSessionByIdAsync(int id)
        {
            var session = await InterviewSessions
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null)
                throw new KeyNotFoundException($"InterviewSession with ID {id} was not found.");

            return session;
        }

        public async Task UpdateInterviewSessionAsync(InterviewSession updated)
        {
            var existing = await InterviewSessions.FindAsync(updated.Id);
            if (existing == null) return;

            existing.InterviewerId = updated.InterviewerId;
            existing.PositionId = updated.PositionId;
            existing.ExternalUserId = updated.ExternalUserId;
            existing.Status = updated.Status;
            existing.DateStart = updated.DateStart;
            existing.Video = updated.Video;
            existing.Score = updated.Score;

            await SaveChangesAsync();
        }
    }
}