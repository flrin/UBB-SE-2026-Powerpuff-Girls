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

namespace Tests_and_Interviews.ViewModels
{
    public class InterviewCandidateViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public InterviewCandidateService Service { get; }
        private string _questionText;
        private string _recordingFilePath;

        public string RecordingFilePath{ get; set; }

        public ICommand NextQuestionCommand;
        public ICommand SubmitRecordingCommand;

        public InterviewCandidateViewModel() {
            Service = new InterviewCandidateService();
            _questionText = "Questions will start after starting recording";
            NextQuestionCommand = new RelayCommand(NextQuestion);
            SubmitRecordingCommand = new RelayCommand(SubmitRecording);
        }

        private void NextQuestion() {
            QuestionText = Service.GetNextQuestion();
        }

        public void StartQuestions()
        {
            QuestionText = Service.GetNextQuestion();
        }

        public void ResetQuestions()
        {
            Service.ResetQuestions() ;
            QuestionText = "Questions will start after starting recording";
        }

        private void SubmitRecording()
        {
            Service.SubmitRecording(RecordingFilePath);
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
