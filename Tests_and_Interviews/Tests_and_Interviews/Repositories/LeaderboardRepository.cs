using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Services;

namespace Tests_and_Interviews.Repositories
{
    public class LeaderboardRepository
    {
        private readonly AppDbContext _db;

        public LeaderboardRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<LeaderboardEntry>> FindByTestIdAsync(int testId)
        {
            return await _db.LeaderboardEntries
                .Include(le => le.User)
                .Include(le => le.Test)
                .Where(le => le.TestId == testId)
                .OrderBy(le => le.RankPosition)
                .ToListAsync();
        }

        public async Task<List<LeaderboardEntry>> FindTopByTestIdAsync(int testId, int limit)
        {
            return await _db.LeaderboardEntries
                .Include(le => le.User)
                .Include(le => le.Test)
                .Where(le => le.TestId == testId)
                .OrderBy(le => le.RankPosition)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<LeaderboardEntry?> FindUserEntryAsync(int userId, int testId)
        {
            return await _db.LeaderboardEntries
                .Include(le => le.User)
                .Include(le => le.Test)
                .FirstOrDefaultAsync(le => le.UserId == userId && le.TestId == testId);
        }

        public async Task DeleteByTestIdAsync(int testId)
        {
            var entries = await _db.LeaderboardEntries
                .Where(le => le.TestId == testId)
                .ToListAsync();

            if (entries.Count == 0)
                return;

            _db.LeaderboardEntries.RemoveRange(entries);
            await _db.SaveChangesAsync();
        }

        public async Task SaveRangeAsync(List<LeaderboardEntry> entries)
        {
            await _db.LeaderboardEntries.AddRangeAsync(entries);
            await _db.SaveChangesAsync();
        }
    }
}