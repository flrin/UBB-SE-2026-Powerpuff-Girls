using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Services;


namespace Tests_and_Interviews.Repositories
{
    public class TestAttemptRepository
    { 
        private readonly AppDbContext _db;

        public TestAttemptRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<TestAttempt?> FindByUserAndTestAsync(int userId, int testId)
        {
            return await _db.TestAttempts
                .Include(ta => ta.Answers)
                .FirstOrDefaultAsync(ta => ta.ExternalUserId == userId && ta.TestId == testId);


        }

        public async Task SaveAsync(TestAttempt attempt)
        {
            _db.TestAttempts.Add(attempt);
            await _db.SaveChangesAsync();
        }

        public async Task<TestAttempt?> UpdateAsync(TestAttempt attempt)
        {
            var existing = await _db.TestAttempts.FindAsync(attempt.Id);
            if (existing == null) return null;

            existing.Score = attempt.Score;
            existing.Status = attempt.Status;
            existing.CompletedAt = attempt.CompletedAt;
            existing.AnswersFilePath = attempt.AnswersFilePath;
            existing.IsValidated = attempt.IsValidated;
            existing.PercentageScore = attempt.PercentageScore;
            existing.RejectionReason = attempt.RejectionReason;
            existing.RejectedAt = attempt.RejectedAt;

            await _db.SaveChangesAsync();
            return existing;
        }


    }

}