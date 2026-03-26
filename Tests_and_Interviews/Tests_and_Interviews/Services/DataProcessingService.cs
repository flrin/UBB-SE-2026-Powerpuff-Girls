using System;
using System.Threading.Tasks;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Repositories;

namespace Tests_and_Interviews.Services
{
    public class DataProcessingService
    {
        private readonly UserRepository _userRepository;
        private readonly TestAttemptRepository _attemptRepository;
        private readonly TestRepository _testRepository;

        // Injected UserRepository instead of AppDbContext
        public DataProcessingService(
            UserRepository userRepository,
            TestAttemptRepository attemptRepository,
            TestRepository testRepository)
        {
            _userRepository = userRepository;
            _attemptRepository = attemptRepository;
            _testRepository = testRepository;
        }

        public async Task<bool> ProcessFinalizedAttemptAsync(int attemptId)
        {
            // Fetching via repository instead of EF Core DbContext
            var attempt = await _attemptRepository.FindByIdAsync(attemptId);

            if (attempt == null)
            {
                return false;
            }

            var validationError = await ValidateAttemptAsync(attempt);

            if (validationError != null)
            {
                attempt.IsValidated = false;
                attempt.PercentageScore = null;
                attempt.RejectionReason = validationError;
                attempt.RejectedAt = DateTime.UtcNow;

                await _attemptRepository.UpdateAsync(attempt);
                return false;
            }

            attempt.IsValidated = true;
            // Assuming Score is a decimal?. Using GetValueOrDefault() to handle potential nulls for math operations.
            attempt.PercentageScore = ConvertToPercentageScore(attempt.Score.GetValueOrDefault());
            attempt.RejectionReason = null;
            attempt.RejectedAt = null;

            await _attemptRepository.UpdateAsync(attempt);
            return true;
        }

        private async Task<string?> ValidateAttemptAsync(TestAttempt attempt)
        {
            // Null check for nullable foreign keys before checking the DB
            if (attempt.ExternalUserId == null)
                return "User does not exist.";

            // Replaced _db.Users.AnyAsync with a repository lookup
            var user = await _userRepository.GetByIdAsync(attempt.ExternalUserId.Value);
            if (user == null)
                return "User does not exist.";

            var test = await _testRepository.FindByIdAsync(attempt.TestId);
            if (test == null)
                return "Test does not exist.";

            if (attempt.CompletedAt == null)
                return "Attempt is incomplete. Missing completion time.";

            if (string.IsNullOrWhiteSpace(attempt.Status))
                return "Attempt status is missing.";

            if (!string.Equals(attempt.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
                return "Attempt is not eligible for leaderboard because status is not COMPLETED.";

            if (attempt.Score < 0 || attempt.Score > 100)
                return "Attempt score is invalid.";

            if (!IsTestStillValidForLeaderboard(test))
                return "Test is no longer valid for leaderboard inclusion.";

            return null;
        }

        private bool IsTestStillValidForLeaderboard(Test test)
        {
            return test.CreatedAt.AddMonths(3) >= DateTime.UtcNow;
        }

        private decimal ConvertToPercentageScore(decimal originalScore)
        {
            return (originalScore / 100m) * 100m;
        }
    }
}