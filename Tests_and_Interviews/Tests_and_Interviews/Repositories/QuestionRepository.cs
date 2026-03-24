using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Services;

namespace Tests_and_Interviews.Repositories
{
    public class QuestionRepository
    {
        private readonly AppDbContext _db;
        public QuestionRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<List<Question>> FindByTestIdAsync(int testId)
        {
            return await _db.Questions
                .Include(q => q.Answers)
                .Where(q => q.TestId ==testId)
                .ToListAsync();
        }
    }
}

