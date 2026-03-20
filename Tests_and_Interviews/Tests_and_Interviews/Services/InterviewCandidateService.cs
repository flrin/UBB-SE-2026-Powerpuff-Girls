using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Helpers;

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
            _interviewSessionId = 1; // this must be passed on by something else
            LoadData();
        }

        private async void LoadData()
        {
            // InterviewSession
            Task<InterviewSession> interviewSessionLoadingTask = _dbContext.GetInterviewSessionByIdAsync(_interviewSessionId);
            _interviewSession = await interviewSessionLoadingTask;
            _interviewSession.DateStart = DateTime.UtcNow;
            _interviewSession.Status = InterviewSessionStatus.RECORDING;

            await _dbContext.UpdateInterviewSessionAsync(_interviewSession);

            // Questions
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
        }
    }
}
