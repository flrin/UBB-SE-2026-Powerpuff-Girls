using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Tests_and_Interviews.Helpers;
using Windows.Graphics.Printing.PrintTicket;

using Tests_and_Interviews.Services;
using Tests_and_Interviews.Models.Core;

using System.Diagnostics;
using Tests_and_Interviews.Models.Enums;

namespace Tests_and_Interviews.ViewModels
{
    public class InterviewCandidateViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _questionText;
        private string _recordingFilePath;
        private List<Question> _questions = new List<Question>();
        private int _currentQuestionIndex = 0;
        private InterviewSession _session;
        private AppDbContext _dbContext;

        public string RecordingFilePath { get; set; }

        public ICommand NextQuestionCommand;
        public ICommand SubmitRecordingCommand;

        public InterviewCandidateViewModel()
        {
            _dbContext = new AppDbContext();
            _questionText = "Questions will start after starting recording";
            NextQuestionCommand = new RelayCommand(NextQuestion);
            SubmitRecordingCommand = new RelayCommand(SubmitRecording);
            // default initialize with session id 1 to keep previous behavior
            _ = InitializeAsync(1);
        }

        public InterviewCandidateViewModel(int interviewSessionId)
        {
            _dbContext = new AppDbContext();
            _questionText = "Questions will start after starting recording";
            NextQuestionCommand = new RelayCommand(NextQuestion);
            SubmitRecordingCommand = new RelayCommand(SubmitRecording);
            _ = InitializeAsync(interviewSessionId);
        }

        private async Task InitializeAsync(int interviewSessionId)
        {
            try
            {
                _session = await _dbContext.GetInterviewSessionByIdAsync(interviewSessionId);
                if (_session != null)
                {
                    _session.DateStart = DateTime.UtcNow;
                    await _dbContext.UpdateInterviewSessionAsync(_session);

                    _questions = await _dbContext.GetInterviewQuestionsByPositionAsync(_session.PositionId);
                    _currentQuestionIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InterviewCandidateViewModel.InitializeAsync failed: {ex.Message}");
            }
        }

        private void NextQuestion()
        {
            QuestionText = GetNextQuestion();
        }

        public void StartQuestions()
        {
            QuestionText = GetNextQuestion();
        }

        public void ResetQuestions()
        {
            _currentQuestionIndex = 0;
            QuestionText = "Questions will start after starting recording";
        }

        private string GetNextQuestion()
        {
            if (_questions == null || _currentQuestionIndex >= _questions.Count)
            {
                return "Congratulation! You finnished all the questions. You may stop and submit the recording now.";
            }
            return _questions[_currentQuestionIndex++].QuestionText;
        }

        private async void SubmitRecording()
        {
            if (_session == null) return;
            try
            {
                _session.Video = RecordingFilePath;
                _session.Status = InterviewStatus.InProgress.ToString();
                await _dbContext.UpdateInterviewSessionAsync(_session);
                try
                {
                    var notif = new NotificationService();
                    notif.ShowSimpleNotification("Video uploaded", "Your interview video was uploaded successfully.");
                }
                catch { }
            }
            catch { }
        }

        public string QuestionText
        {
            get { return _questionText; }
            set
            {
                if (_questionText != value)
                {
                    _questionText = value;
                    OnPropertyChanged();
                }
            }
        }

    }
}
