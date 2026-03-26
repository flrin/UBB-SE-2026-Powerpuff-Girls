using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Tests_and_Interviews.Helpers;
using Tests_and_Interviews.Services;
using Tests_and_Interviews.Models.Enums;
using Tests_and_Interviews.Repositories;

namespace Tests_and_Interviews.ViewModels
{
    public class InterviewInterviewerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand SubmitScoreCommand { get; }

        private readonly InterviewSessionRepository _sessionRepo;
        private int _sessionId;
        private Uri _recordingUri;
        private float _score;

        public Uri RecordingUri
        {
            get { return _recordingUri; }
            set
            {
                if (_recordingUri != value)
                {
                    _recordingUri = value;
                    OnPropertyChanged();
                }
            }
        }

        public float Score
        {
            get { return _score; }
            set
            {
                if (_score != value)
                {
                    _score = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Inject the ADO.NET repository
        public InterviewInterviewerViewModel()
        {
            _sessionRepo = new InterviewSessionRepository();
            SubmitScoreCommand = new RelayCommand(_ => SubmitScore());

            _recordingUri = new Uri("about:blank");
            _score = 1.0f;
        }

        // Made async to properly await the repository call
        public async void InitializeSession(int interviewSessionId)
        {
            _sessionId = interviewSessionId;
            try
            {
                var session = await _sessionRepo.GetInterviewSessionByIdAsync(interviewSessionId);
                string videoPath = session?.Video ?? string.Empty;

                if (string.IsNullOrWhiteSpace(videoPath))
                {
                    RecordingUri = new Uri("about:blank");
                }
                else
                {
                    string localFolderPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
                    if (videoPath.StartsWith(localFolderPath, StringComparison.OrdinalIgnoreCase))
                    {
                        string relativePath = videoPath.Substring(localFolderPath.Length).Replace('\\', '/');
                        if (!relativePath.StartsWith("/")) relativePath = "/" + relativePath;
                        RecordingUri = new Uri($"ms-appdata:///local{relativePath}");
                    }
                    else if (!System.IO.Path.IsPathRooted(videoPath))
                    {
                        string relativePath = videoPath.Replace('\\', '/');
                        if (!relativePath.StartsWith("/")) relativePath = "/" + relativePath;
                        RecordingUri = new Uri($"ms-appdata:///local{relativePath}");
                    }
                    else
                    {
                        RecordingUri = new Uri(videoPath);
                    }
                }
            }
            catch
            {
                RecordingUri = new Uri("about:blank");
            }
        }

        // Made async to ensure the database update completes
        public async void SubmitScore()
        {
            try
            {
                var session = await _sessionRepo.GetInterviewSessionByIdAsync(_sessionId);
                if (session != null)
                {
                    session.Score = (decimal)Score;
                    session.Status = InterviewStatus.Completed.ToString();
                    await _sessionRepo.UpdateInterviewSessionAsync(session);
                }

                try
                {
                    var notif = new NotificationService();
                    notif.ShowSimpleNotification("Score submitted", "The interview score was submitted successfully.");
                }
                catch { }
            }
            catch { }
        }
    }
}