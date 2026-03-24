using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Services;


namespace Tests_and_Interviews.Repositories
{
    public class AnswerRepository
    {
        private readonly AppDbContext _db;
        public AnswerRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task SaveAsync(Answer answer)
        {
            _db.Answers.Add(answer);
            await _db.SaveChangesAsync();
        }

        public async Task<List<Answer>> FindByAttemptAsync(int attemptId)
        {
            return await _db.Answers
                .Include(a => a.Question)
                .Where(a => a.AttemptId == attemptId)
                .ToListAsync();
        }
    }
}