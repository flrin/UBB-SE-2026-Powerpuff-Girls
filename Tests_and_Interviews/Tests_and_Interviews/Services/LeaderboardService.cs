using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Repositories;

namespace Tests_and_Interviews.Services
{
    public class LeaderboardService
    {
        private readonly AppDbContext _db;
        private readonly LeaderboardRepository _leaderboardRepository;

        public LeaderboardService(AppDbContext db, LeaderboardRepository leaderboardRepository)
        {
            _db = db;
            _leaderboardRepository = leaderboardRepository;
        }

        public async Task RecalculateLeaderboardAsync(int testId)
        {
            var attempts = await _db.TestAttempts
                .Include(a => a.User)
                .Where(a =>
                    a.TestId == testId &&
                    a.Status == "COMPLETED" &&
                    a.IsValidated &&
                    a.PercentageScore != null &&
                    a.CompletedAt != null)
                .OrderByDescending(a => a.PercentageScore)
                .ThenBy(a => a.CompletedAt)
                .ToListAsync();

            await _leaderboardRepository.DeleteByTestIdAsync(testId);

            var entries = new List<LeaderboardEntry>();

            for (int i = 0; i < attempts.Count; i++)
            {
                var attempt = attempts[i];

                entries.Add(new LeaderboardEntry
                {
                    TestId = attempt.TestId,
                    UserId = attempt.ExternalUserId,
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