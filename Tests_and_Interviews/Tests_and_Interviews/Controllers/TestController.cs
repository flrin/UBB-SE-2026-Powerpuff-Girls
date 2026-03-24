using System.Collections.Generic;
using System.Threading.Tasks;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Services;

namespace Tests_and_Interviews.Controllers
{

    public class TestController
    {
        private readonly TestService _testService;
        private readonly TimerService _timerService;

        public TestController(TestService testService, TimerService timerService)
        {
            _testService = testService;
            _timerService = timerService;
        }

        public async Task StartTestAsync(int userId, int testId)
        {
            await _testService.StartTestAsync(userId, testId);
        }

        public async Task SubmitTestAsync(int attemptId)
        {
            await _testService.SubmitTestAsync(attemptId);
        }

        public async Task<List<Test>> GetAvailableTestsAsync(string category)
        {
            var tests = new List<Test>();

            var next = await _testService.GetNextAvailableTestAsync(category);
            if (next != null)
                tests.Add(next);

            return tests;
        }
        public async Task RemoveExpiredTestsAsync(int attemptId)
        {
            if (_timerService.CheckExpiration(attemptId))
                await _timerService.ExpireTestAsync(attemptId);
        }

        public async Task ReplaceExpiredTestsAsync()
        {
            var expiredIds = _timerService.GetExpiredAttemptIds();

            foreach (var attemptId in expiredIds)
                await _timerService.ExpireTestAsync(attemptId);
        }
    }
}