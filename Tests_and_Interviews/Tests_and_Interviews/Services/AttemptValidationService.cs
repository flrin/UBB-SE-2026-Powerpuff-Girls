using System.Threading.Tasks;
using Tests_and_Interviews.Models.Enums;
using Tests_and_Interviews.Repositories;

namespace Tests_and_Interviews.Services
{ 
    public class AttemptValidationService
    {
        private readonly TestAttemptRepository _attemptRepository;

        public AttemptValidationService(TestAttemptRepository attemptRepository)
        {
            _attemptRepository = attemptRepository;
        }

        
        public async Task<bool> CanStartTestAsync(int userId, int testId)
        {
            var existing = await _attemptRepository.FindByUserAndTestAsync(userId, testId);

            if (existing == null)
                return true;

            return existing.Status == TestStatus.NOT_STARTED.ToString();
        }

       
        public async Task CheckExistingAttemptsAsync(int userId, int testId)
        {
            var existing = await _attemptRepository.FindByUserAndTestAsync(userId, testId);

            if (existing == null) return;

            if (existing.Status == TestStatus.SUBMITTED.ToString() ||
                existing.Status == TestStatus.REVIEWED.ToString())
            {
                throw new System.InvalidOperationException(
                    $"User {userId} has already completed test {testId}.");
            }
        }
    }
}