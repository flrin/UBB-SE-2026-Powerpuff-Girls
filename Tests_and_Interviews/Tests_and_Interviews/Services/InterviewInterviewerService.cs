using System;
using System.IO;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Models.Enums;
using Tests_and_Interviews.Repositories;
using Windows.Storage;

namespace Tests_and_Interviews.Services
{
    public class InterviewInterviewerService
    {
        private readonly InterviewSessionRepository _interviewSessionRepo;
        private int _interviewSessionId;
        private InterviewSession _interviewSession;

        public InterviewInterviewerService(InterviewSessionRepository interviewSessionRepo)
        {
            _interviewSessionRepo = interviewSessionRepo;
            _interviewSessionId = 1;
            LoadData();
        }

        public void LoadData()
        {
            _interviewSession = _interviewSessionRepo.GetInterviewSessionById(_interviewSessionId);
        }

        public string GetRecordingPath()
        {
            if (_interviewSession == null || string.IsNullOrWhiteSpace(_interviewSession.Video))
            {
                return string.Empty;
            }

            string videoPath = _interviewSession.Video;
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
            _interviewSession.Status = InterviewStatus.Completed.ToString();

            await _interviewSessionRepo.UpdateInterviewSessionAsync(_interviewSession);
            try
            {
                var notif = new NotificationService();
                notif.ShowSimpleNotification("Score submitted", "The interview score was submitted successfully.");
            }
            catch { }
        }
    }
}