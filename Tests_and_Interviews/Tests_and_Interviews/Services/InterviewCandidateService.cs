using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Helpers;
using Tests_and_Interviews.Models.Core;

namespace Tests_and_Interviews.Services
{
    public class InterviewCandidateService
    {
        public List<Question> questions { get; set; }
        private int _currentQuestionIndex = 0;
        private readonly AppDbContext _dbContext;
        private InterviewSession _interviewSession;
        private int _interviewSessionId;

        public InterviewCandidateService()
        {
            _dbContext = new AppDbContext();
            _interviewSessionId = 1; 
            LoadData();
        }

        private async void LoadData()
        {
            Task<InterviewSession> interviewSessionLoadingTask = _dbContext.GetInterviewSessionByIdAsync(_interviewSessionId);
            _interviewSession = await interviewSessionLoadingTask;
            _interviewSession.DateStart = DateTime.UtcNow;
            _interviewSession.Status = InterviewSessionStatus.RECORDING;

            await _dbContext.UpdateInterviewSessionAsync(_interviewSession);

            Task<List<Question>> questionsLoadingTask = _dbContext.GetInterviewQuestionsByPositionAsync(_interviewSession.PositionId);
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
            _interviewSession.Status = InterviewSessionStatus.SUBMITTED;

            await _dbContext.UpdateInterviewSessionAsync(_interviewSession);
            try
            {
                var notif = new NotificationService();
                notif.ShowSimpleNotification("Video uploaded", "Your interview video was uploaded successfully.");
            }
            catch { }
        }
    }
}
