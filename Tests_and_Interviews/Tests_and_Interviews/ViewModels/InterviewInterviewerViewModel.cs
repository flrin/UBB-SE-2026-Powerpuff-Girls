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

namespace Tests_and_Interviews.ViewModels
{
    public class InterviewInterviewerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand SubmitScoreCommand;
        public InterviewInterviewerService Service { get; }
        public Uri RecordingUri { 
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
        private Uri _recordingUri;
        private float _score;
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

        public InterviewInterviewerViewModel() 
        {
            SubmitScoreCommand = new RelayCommand(SubmitScore);
            Service = new InterviewInterviewerService();
            string _recordingPath = Service.GetRecordingPath();
            _recordingUri = string.IsNullOrWhiteSpace(_recordingPath) ? new Uri("about:blank") : new Uri(_recordingPath);
            _score = 1.0f;
        }

        private int _sessionId;

        public void InitializeSession(int interviewSessionId)
        {
            _sessionId = interviewSessionId;
            try
            {
                var db = new Services.AppDbContext();
                var session = db.GetInterviewSessionById(interviewSessionId);
                string videoPath = session?.Video ?? string.Empty;
                if (string.IsNullOrWhiteSpace(videoPath))
                {
                    _recordingUri = new Uri("about:blank");
                }
                else
                {
                    // reuse logic similar to InterviewInterviewerService.GetRecordingPath
                    string localFolderPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
                    if (videoPath.StartsWith(localFolderPath, StringComparison.OrdinalIgnoreCase))
                    {
                        string relativePath = videoPath.Substring(localFolderPath.Length).Replace('\\', '/');
                        if (!relativePath.StartsWith("/")) relativePath = "/" + relativePath;
                        _recordingUri = new Uri($"ms-appdata:///local{relativePath}");
                    }
                    else if (!System.IO.Path.IsPathRooted(videoPath))
                    {
                        string relativePath = videoPath.Replace('\\', '/');
                        if (!relativePath.StartsWith("/")) relativePath = "/" + relativePath;
                        _recordingUri = new Uri($"ms-appdata:///local{relativePath}");
                    }
                    else
                    {
                        _recordingUri = new Uri(videoPath);
                    }
                }
            }
            catch
            {
                _recordingUri = new Uri("about:blank");
            }
            OnPropertyChanged(nameof(RecordingUri));
        }

        public void SubmitScore()
        {
            try
            {
                var db = new Services.AppDbContext();
                var session = db.GetInterviewSessionById(_sessionId);
                if (session != null)
                {
                    session.Score = (decimal)Score;
                    session.Status = InterviewStatus.Completed.ToString();
                    db.UpdateInterviewSessionAsync(session);
                }

                try
                {
                    var notif = new Services.NotificationService();
                    notif.ShowSimpleNotification("Score submitted", "The interview score was submitted successfully.");
                }
                catch { }
            }
            catch { }
        }
    }
}
