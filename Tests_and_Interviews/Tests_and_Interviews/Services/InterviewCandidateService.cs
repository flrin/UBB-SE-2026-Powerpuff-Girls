using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Models.Enums;
using Tests_and_Interviews.Repositories;

namespace Tests_and_Interviews.Services
{
    public class InterviewCandidateService
    {
        public List<Question> questions { get; set; }
        private int _currentQuestionIndex = 0;

        private readonly InterviewSessionRepository _interviewSessionRepo;
        private readonly QuestionRepository _questionRepo;

        private InterviewSession _interviewSession;
        private int _interviewSessionId;

        public InterviewCandidateService()
        {
            _interviewSessionRepo = new InterviewSessionRepository();
            _questionRepo = new QuestionRepository();
            _interviewSessionId = 1;
            LoadData();
        }

        private async void LoadData()
        {
            Task<InterviewSession> interviewSessionLoadingTask = _interviewSessionRepo.GetInterviewSessionByIdAsync(_interviewSessionId);
            _interviewSession = await interviewSessionLoadingTask;
            _interviewSession.DateStart = DateTime.UtcNow;

            await _interviewSessionRepo.UpdateInterviewSessionAsync(_interviewSession);

            Task<List<Question>> questionsLoadingTask = _questionRepo.GetInterviewQuestionsByPositionAsync(_interviewSession.PositionId);
            questions = await questionsLoadingTask;
        }

        public string GetNextQuestion()
        {
            if (_currentQuestionIndex >= questions.Count)
            {
                return "Congratulation! You finnished all the questions. You may stop and submit the recording now.";
            }
            return questions[_currentQuestionIndex++].QuestionText;
        }

        public void ResetQuestions()
        {
            _currentQuestionIndex = 0;
        }

        public async void SubmitRecording(string recordingFilePath)
        {
            _interviewSession.Video = recordingFilePath;
            _interviewSession.Status = InterviewStatus.InProgress.ToString();

            await _interviewSessionRepo.UpdateInterviewSessionAsync(_interviewSession);
            try
            {
                var notif = new NotificationService();
                notif.ShowSimpleNotification("Video uploaded", "Your interview video was uploaded successfully.");
            }
            catch { }
        }
    }
}