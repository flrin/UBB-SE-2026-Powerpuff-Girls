using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Models.Enums;
using Tests_and_Interviews.Repositories;


namespace Tests_and_Interviews.Services
{
    public class TimerService
    {
        private static readonly ConcurrentDictionary<int, DateTime> _timers = new();
        private static readonly TimeSpan TestDuration = TimeSpan.FromMinutes(30);
        private readonly TestAttemptRepository _testAttemptRepository;
        public TimerService(TestAttemptRepository testAttemptRepository)
        {
            _testAttemptRepository = testAttemptRepository;
        }

        public void StartTimer(int attemptId)
        {
            _timers[attemptId] = DateTime.UtcNow;
        }

        public bool CheckExpiration(int attemptId)
        {
            if (!_timers.TryGetValue(attemptId, out DateTime startTime))
                return false;

            return DateTime.UtcNow - startTime > TestDuration;
        }

        public List<int> GetExpiredAttemptIds()
        {
            var expired = new List<int>();
            foreach (var kvp in _timers)
            {
                if (DateTime.UtcNow - kvp.Value > TestDuration)
                    expired.Add(kvp.Key);
            }
            return expired;
        }

        public async Task ExpireTestAsync(int attemptId)
        {
            var expiredAttempt = new TestAttempt
            {
                Id = attemptId,
                Status = TestStatus.SUBMITTED.ToString(),
                CompletedAt = DateTime.UtcNow
            };

            await _testAttemptRepository.UpdateAsync(expiredAttempt);
            _timers.TryRemove(attemptId, out _);
        }
    }
}