using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Models.Enums;
using Tests_and_Interviews.Repositories;
using Tests_and_Interviews.Services;

namespace Tests_and_Interviews.ViewModels
{
    public class OptionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        void Notify([CallerMemberName] string p = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

        public string Text { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public int Index { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; Notify(); OnSelectionChanged?.Invoke(); }
        }
        public Action? OnSelectionChanged { get; set; }
    }

    public class QuestionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        void Notify([CallerMemberName] string p = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

        public int QuestionId { get; set; }
        public int DisplayNumber { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public string TypeLabel => Type.ToString().Replace("_", " ");

        public ObservableCollection<OptionViewModel> Options { get; set; } = new();

        public Visibility IsSingleChoice => Type == QuestionType.SINGLE_CHOICE ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsMultipleChoice => Type == QuestionType.MULTIPLE_CHOICE ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsTrueFalse => Type == QuestionType.TRUE_FALSE ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsText => Type == QuestionType.TEXT ? Visibility.Visible : Visibility.Collapsed;

        public string TrueFalseGroup => $"tf_{QuestionId}";

        private bool _trueSelected;
        public bool TrueSelected
        {
            get => _trueSelected;
            set
            {
                _trueSelected = value;
                if (value) _falseSelected = false;
                Notify(); Notify(nameof(FalseSelected));
                OnAnswerChanged?.Invoke();
            }
        }

        private bool _falseSelected;
        public bool FalseSelected
        {
            get => _falseSelected;
            set
            {
                _falseSelected = value;
                if (value) _trueSelected = false;
                Notify(); Notify(nameof(TrueSelected));
                OnAnswerChanged?.Invoke();
            }
        }

        private string _textAnswer = string.Empty;
        public string TextAnswer
        {
            get => _textAnswer;
            set { _textAnswer = value; Notify(); OnAnswerChanged?.Invoke(); }
        }

        public Action? OnAnswerChanged { get; set; }

        public string GetAnswerValue()
        {
            return Type switch
            {
                QuestionType.SINGLE_CHOICE => Options.FirstOrDefault(o => o.IsSelected)?.Index.ToString() ?? string.Empty,
                QuestionType.MULTIPLE_CHOICE =>
                    "[" + string.Join(",", Options.Where(o => o.IsSelected).Select(o => o.Index)) + "]",
                QuestionType.TRUE_FALSE => TrueSelected ? "true" : FalseSelected ? "false" : string.Empty,
                QuestionType.TEXT => TextAnswer.Trim(),
                _ => string.Empty
            };
        }

        public bool IsAnswered()
        {
            return Type switch
            {
                QuestionType.SINGLE_CHOICE => Options.Any(o => o.IsSelected),
                QuestionType.MULTIPLE_CHOICE => Options.Any(o => o.IsSelected),
                QuestionType.TRUE_FALSE => TrueSelected || FalseSelected,
                QuestionType.TEXT => !string.IsNullOrWhiteSpace(TextAnswer),
                _ => false
            };
        }
    }

    public class TestPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        void Notify([CallerMemberName] string p = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

        public ObservableCollection<QuestionViewModel> Questions { get; } = new();

        private string _testTitle = string.Empty;
        public string TestTitle { get => _testTitle; set { _testTitle = value; Notify(); } }

        private TimeSpan _timeLeft = TimeSpan.FromMinutes(30);
        private DispatcherTimer? _timer;
        public string TimerDisplay => _timeLeft.ToString(@"mm\:ss");
        public Action? OnTimerExpired { get; set; }

        private int _answeredCount;
        public int AnsweredCount { get => _answeredCount; set { _answeredCount = value; Notify(); } }
        public int TotalCount => Questions.Count;

        public bool AlreadyAttempted { get; private set; } = false;

        // Repositories & Services
        private readonly UserRepository _userRepo;
        private readonly TestRepository _testRepo;
        private readonly QuestionRepository _questionRepo;
        private readonly TestAttemptRepository _attemptRepo;
        private readonly AnswerRepository _answerRepo;
        private readonly TestService _testService;
        private readonly DataProcessingService _dataProcessingService;

        private int _attemptId;
        public int UserId { get; set; }
        public int TestId { get; set; }

        // Inject all required repositories via constructor
        public TestPageViewModel()
        {
            _userRepo = new UserRepository();
            _testRepo = new TestRepository();
            _questionRepo = new QuestionRepository();
            _attemptRepo = new TestAttemptRepository();
            _answerRepo = new AnswerRepository();

            // Instantiate internal domain services using the injected ADO.NET repositories
            var grading = new GradingService();
            var timerSvc = new TimerService(_attemptRepo);
            var validation = new AttemptValidationService(_attemptRepo);

            _testService = new TestService(_testRepo, _attemptRepo, _answerRepo, grading, timerSvc, validation);
            _dataProcessingService = new DataProcessingService(_userRepo, _attemptRepo, _testRepo);
        }

        public async System.Threading.Tasks.Task LoadAsync(int testId, int userId)
        {
            TestId = testId;

            // If a valid userId is passed in, use it. Otherwise, fallback to finding Alice for dev purposes.
            if (userId > 0)
            {
                UserId = userId;
            }
            else
            {
                var users = await _userRepo.GetAllAsync();
                var user = users.FirstOrDefault(u => u.Name == "Alice Johnson");
                UserId = user?.Id ?? 0;
            }

            System.Diagnostics.Debug.WriteLine($"[TestPageViewModel] UserId = {UserId}");

            var test = await _testRepo.FindByIdAsync(testId);
            if (test == null) return;

            TestTitle = test.Title;

            try
            {
                await _testService.StartTestAsync(UserId, testId);
            }
            catch (InvalidOperationException)
            {
                AlreadyAttempted = true;
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StartTest error] {ex.InnerException?.Message ?? ex.Message}");
            }

            var questions = await _questionRepo.FindByTestIdAsync(testId);

            int idx = 1;
            foreach (var q in questions)
            {
                if (q.Type == QuestionType.INTERVIEW) continue;

                var qvm = new QuestionViewModel
                {
                    QuestionId = q.Id,
                    DisplayNumber = idx++,
                    QuestionText = q.QuestionText,
                    Type = q.Type,
                };

                if (q.Type == QuestionType.SINGLE_CHOICE || q.Type == QuestionType.MULTIPLE_CHOICE)
                {
                    List<string> optionLabels;
                    if (!string.IsNullOrEmpty(q.OptionsJson))
                    {
                        optionLabels = System.Text.Json.JsonSerializer.Deserialize<List<string>>(q.OptionsJson)
                                       ?? new List<string> { "Option A", "Option B", "Option C", "Option D", "Option E", "Option F" };
                    }
                    else
                    {
                        optionLabels = new List<string> { "Option A", "Option B", "Option C", "Option D", "Option E", "Option F" };
                    }
                    for (int i = 0; i < optionLabels.Count; i++)
                    {
                        var opt = new OptionViewModel
                        {
                            Text = optionLabels[i],
                            Index = i,
                            GroupName = $"q_{q.Id}"
                        };
                        opt.OnSelectionChanged = UpdateAnsweredCount;
                        qvm.Options.Add(opt);
                    }
                }

                qvm.OnAnswerChanged = UpdateAnsweredCount;
                Questions.Add(qvm);
            }

            Notify(nameof(TotalCount));
            StartTimer();
        }

        void StartTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                _timeLeft = _timeLeft.Subtract(TimeSpan.FromSeconds(1));
                Notify(nameof(TimerDisplay));
                if (_timeLeft <= TimeSpan.Zero)
                {
                    _timer.Stop();
                    OnTimerExpired?.Invoke();
                }
            };
            _timer.Start();
        }

        public void StopTimer() => _timer?.Stop();

        void UpdateAnsweredCount()
        {
            AnsweredCount = Questions.Count(q => q.IsAnswered());
        }

        public async System.Threading.Tasks.Task<float> SubmitAsync()
        {
            StopTimer();

            var attempt = await _attemptRepo.FindByUserAndTestAsync(UserId, TestId);
            if (attempt == null) return 0f;

            _attemptId = attempt.Id;

            foreach (var qvm in Questions)
            {
                var val = qvm.GetAnswerValue();
                if (string.IsNullOrEmpty(val)) continue;
                var answer = new Answer
                {
                    AttemptId = _attemptId,
                    QuestionId = qvm.QuestionId,
                    Value = val
                };
                await _answerRepo.SaveAsync(answer);
            }

            await _testService.SubmitTestAsync(_attemptId);

            await _dataProcessingService.ProcessFinalizedAttemptAsync(_attemptId);

            var finalAttempt = await _attemptRepo.FindByUserAndTestAsync(UserId, TestId);
            return finalAttempt != null ? (float)(finalAttempt.Score ?? 0m) : 0f;
        }
    }
}