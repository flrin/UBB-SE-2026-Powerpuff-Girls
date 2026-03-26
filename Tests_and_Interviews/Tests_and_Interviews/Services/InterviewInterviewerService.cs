using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests_and_Interviews.Helpers;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Models.Core;
using Windows.Storage;
using System.IO;

namespace Tests_and_Interviews.Services
{
    public class InterviewInterviewerService
    {
        private AppDbContext _dbContext;
        private int _interviewSessionId;
        private InterviewSession _interviewSession;

        public InterviewInterviewerService()
        {
            _dbContext = new AppDbContext();
            _interviewSessionId = 1; // this must be passed on by something else
            LoadData();
        }

        public void LoadData()
        {
            _interviewSession = _dbContext.GetInterviewSessionById(_interviewSessionId);
        }

        public string GetRecordingPath()
        {
            string videoPath = _interviewSession.Video;

            if (string.IsNullOrWhiteSpace(videoPath))
            {
                return string.Empty;
            }

            string localFolderPath = ApplicationData.Current.LocalFolder.Path;

            if (videoPath.StartsWith(localFolderPath, StringComparison.OrdinalIgnoreCase))
            {
                string relativePath = videoPath.Substring(localFolderPath.Length).Replace('\\', '/');
                if (!relativePath.StartsWith("/")) relativePath = "/" + relativePath;
                return $"ms-appdata:///local{relativePath}";
            }

            if (!Path.IsPathRooted(videoPath))
            {
                string relativePath = videoPath.Replace('\\', '/');
                if (!relativePath.StartsWith("/")) relativePath = "/" + relativePath;
                return $"ms-appdata:///local{relativePath}";
            }
            return videoPath;
        }

        public async void SubmitScore(float score) 
        {
            _interviewSession.Score = (decimal)score;
            _interviewSession.Status = InterviewSessionStatus.REVIEWED;
            await _dbContext.UpdateInterviewSessionAsync(_interviewSession);
            try
            {
                var notif = new NotificationService();
                notif.ShowSimpleNotification("Score submitted", "The interview score was submitted successfully.");
            }
            catch { }
        }
    }
}
