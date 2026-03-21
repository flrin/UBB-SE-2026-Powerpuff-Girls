using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Services;

namespace Tests_and_Interviews.Repositories
{
    public class TestRepository
    {
        private readonly AppDbContext _db;

        public TestRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Test?> FindByIdAsync(int id)
        {
            return await _db.Tests
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Test>> FindTestsByCategory(string category)
        {
            return await _db.Tests
                .Include(t => t.Questions)
                .Where(t => t.Category == category)
                .ToListAsync();
        }
    }
}

