using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Models.Core;


namespace Tests_and_Interviews.Services
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<InterviewSession> InterviewSessions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=WinUIDevDb;Username=devuser;Password=devpassword");
        }

        public void SeedDatabase()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();

            if (!Questions.Any())
            {
                Questions.AddRange(
                    new Question { 
                        PositionId = 1,
                        QuestionText = "Tell us about your favorite project." ,
                        QuestionType = "Interview",
                        QuestionAnswer = ""
                    },
                    new Question {
                        PositionId = 1,
                        QuestionText = "How do you handle conflict in a team?" ,
                        QuestionType = "Interview",
                        QuestionAnswer = ""
                    },
                    new Question {
                        PositionId = 2,
                        QuestionText = "Where do you see yourself in 5 years?" ,
                        QuestionType = "Interview",
                        QuestionAnswer = ""
                    },
                    new Question
                    {
                        PositionId = 1,
                        QuestionText = "Does boolean mean true or false?",
                        QuestionType = "T/F",
                        QuestionAnswer = "T"
                    }
                );

                SaveChanges();
            }

            if (!InterviewSessions.Any())
            {
                InterviewSessions.AddRange(
                    new InterviewSession
                    {
                        PositionId= 1,
                        Video = "",
                        Status = "NOT_SUBMITED",
                    }
                );

                SaveChanges();
            }
        }

        public async Task<List<Question>> GetInterviewQuestionsByPositionAsync(int positionId)
        {
            using (var db = new AppDbContext())
            {
                return await db.Questions
                    .Where(q => q.QuestionType == "Interview" && q.PositionId == positionId)
                    .ToListAsync();
            }
        }

        public async Task<InterviewSession> GetInterviewSessionByIdAsync(int id)
        {
            using (var db = new AppDbContext())
            {
                
                var session = await db.InterviewSessions
                    .FirstOrDefaultAsync(ins => ins.Id == id);

                if (session == null)
                {
                    
                    throw new KeyNotFoundException($"InterviewSession with ID {id} was not found.");
                }

                return session;
            }
        }

        public async Task UpdateInterviewSessionAsync(InterviewSession interviewSession)
        {
            using (var db = new AppDbContext())
            {
                var dbInterviewSession = await db.InterviewSessions.FindAsync(interviewSession.Id);
                if ( dbInterviewSession != null)
                {
                    dbInterviewSession.InterviewerId = interviewSession.InterviewerId;
                    dbInterviewSession.PositionId = interviewSession.PositionId;
                    dbInterviewSession.Status = interviewSession.Status;
                    dbInterviewSession.DateStart = interviewSession.DateStart;
                    dbInterviewSession.Video = interviewSession.Video;
                    dbInterviewSession.Score = interviewSession.Score;
                    dbInterviewSession.ExternalUserId = interviewSession.ExternalUserId;
                    dbInterviewSession.Id = interviewSession.Id;

                    await db.SaveChangesAsync();
                }
            }
        }

    }
}
