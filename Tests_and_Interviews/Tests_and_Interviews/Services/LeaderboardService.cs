using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Repositories;

namespace Tests_and_Interviews.Services
{
    public class LeaderboardService
    {
        private readonly TestAttemptRepository _testAttemptRepository;
        private readonly LeaderboardRepository _leaderboardRepository;

        // Injected TestAttemptRepository instead of AppDbContext
        public LeaderboardService()
        {
            _testAttemptRepository = new TestAttemptRepository();
            _leaderboardRepository = new LeaderboardRepository();
        }

        public async Task RecalculateLeaderboardAsync(int testId)
        {
            // Replaced _db.TestAttempts query with repository call
            var attempts = await _testAttemptRepository.FindValidAttemptsByTestIdAsync(testId);

            await _leaderboardRepository.DeleteByTestIdAsync(testId);

            var entries = new List<LeaderboardEntry>();

            for (int i = 0; i < attempts.Count; i++)
            {
                var attempt = attempts[i];

                entries.Add(new LeaderboardEntry
                {
                    TestId = attempt.TestId,
                    // Note: Ensure your external user ID is not null here since it passed the validation filters
                    UserId = attempt.ExternalUserId.Value,
                    NormalizedScore = attempt.PercentageScore!.Value,
                    RankPosition = i + 1,
                    TieBreakPriority = i + 1,
                    LastRecalculationAt = DateTime.UtcNow
                });
            }

            if (entries.Count > 0)
            {
                await _leaderboardRepository.SaveRangeAsync(entries);
            }
        }

        public async Task<List<LeaderboardEntry>> GetTopThreeAsync(int testId)
        {
            await RecalculateLeaderboardAsync(testId);
            return await _leaderboardRepository.FindTopByTestIdAsync(testId, 3);
        }

        public async Task<LeaderboardEntry?> GetUserRankingAsync(int userId, int testId)
        {
            await RecalculateLeaderboardAsync(testId);
            return await _leaderboardRepository.FindUserEntryAsync(userId, testId);
        }

        public async Task<List<LeaderboardEntry>> GetFullLeaderboardAsync(int testId)
        {
            await RecalculateLeaderboardAsync(testId);
            return await _leaderboardRepository.FindByTestIdAsync(testId);
        }
    }
}