using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Models.Enums;
using Tests_and_Interviews.Repositories;

namespace Tests_and_Interviews.Services
{
    public class TestService
    {
        private readonly TestRepository _testRepository;
        private readonly TestAttemptRepository _attemptRepository;
        private readonly AnswerRepository _answerRepository;
        private readonly GradingService _gradingService;
        private readonly TimerService _timerService;
        private readonly AttemptValidationService _validationService;

        public TestService(
            TestRepository testRepository,
            TestAttemptRepository attemptRepository,
            AnswerRepository answerRepository,
            GradingService gradingService,
            TimerService timerService,
            AttemptValidationService validationService)
        {
            _testRepository = testRepository;
            _attemptRepository = attemptRepository;
            _answerRepository = answerRepository;
            _gradingService = gradingService;
            _timerService = timerService;
            _validationService = validationService;
        }

        
        public async Task StartTestAsync(int userId, int testId)
        {
            await _validationService.CheckExistingAttemptsAsync(userId, testId);

            var attempt = new TestAttempt
            {
                TestId = testId,
                ExternalUserId = userId,
                Status = TestStatus.RECORDING.ToString(),
                StartedAt = DateTime.UtcNow
            };

            attempt.Start();
            await _attemptRepository.SaveAsync(attempt);
            _timerService.StartTimer(attempt.Id);
        }

        
        public async Task SubmitTestAsync(int attemptId)
        {
            var answers = await _answerRepository.FindByAttemptAsync(attemptId);
            var attempt = new TestAttempt { Id = attemptId, Answers = answers };

            foreach (var answer in answers)
            {
                if (answer.Question == null) continue;

                switch (answer.Question.Type)
                {
                    case QuestionType.SINGLE_CHOICE:
                        _gradingService.GradeSingleChoice(answer.Question, answer);
                        break;
                    case QuestionType.MULTIPLE_CHOICE:
                        _gradingService.GradeMultipleChoice(answer.Question, answer);
                        break;
                    case QuestionType.TEXT:
                        _gradingService.GradeText(answer.Question, answer);
                        break;
                    case QuestionType.TRUE_FALSE:
                        _gradingService.GradeTrueFalse(answer.Question, answer);
                        break;
                }
            }

            _gradingService.CalculateFinalScore(attempt);

            attempt.Submit();
            await _attemptRepository.UpdateAsync(attempt);
        }

        
        public async Task LoadTestAsync(int attemptId)
        {
            await _answerRepository.FindByAttemptAsync(attemptId);
        }

        
        public async Task<Test?> GetNextAvailableTestAsync(string category)
        {
            var tests = await _testRepository.FindTestsByCategoryAsync(category);

            if (tests.Count == 0)
                return null;

            return tests[0];
        }
    }
}