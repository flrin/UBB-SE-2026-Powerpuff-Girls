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
            _recordingUri = new Uri(_recordingPath);
            _score = 1.0f;
        }

        private void SubmitScore() 
        {
            Service.SubmitScore(Score);
        }
    }
}
