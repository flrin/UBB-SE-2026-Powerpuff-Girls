using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tests_and_Interviews.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Tests_and_Interviews.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InterviewCandidatePage : Page
    {
        private MediaCapture _mediaCapture = new MediaCapture();
        public InterviewCandidateViewModel ViewModel { get; }
        private bool _isRecording = false;
        private StorageFile _recordingFile;
        public InterviewCandidatePage()
        {
            InitializeComponent();
            ViewModel = new InterviewCandidateViewModel();
            StopVideoButton.IsEnabled = false;
            SubmitVideoButton.IsEnabled = false;
            NextQuestionButton.IsEnabled = false;
            RecordingBorder.BorderThickness = new Thickness(0);
            StartCamera();
        }

        private async void StartCamera()
        {
            await _mediaCapture.InitializeAsync();

            var frameSource = _mediaCapture.FrameSources.Values.FirstOrDefault(
                source => source.Info.SourceKind == Windows.Media.Capture.Frames.MediaFrameSourceKind.Color);

            if (frameSource != null)
            {
                captureElement.Source = Windows.Media.Core.MediaSource.CreateFromMediaFrameSource(frameSource);
                captureElement.MediaPlayer.Play();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No valid color video frame source was found.");
            }
        }

        private async void StartRecording_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (_isRecording || _mediaCapture == null) return;

            try
            {
                StartVideoButton.IsEnabled = false;
                StopVideoButton.IsEnabled = true;
                SubmitVideoButton.IsEnabled = false;
                NextQuestionButton.IsEnabled = true;
                RecordingBorder.BorderThickness = new Thickness(10);

                var storageFolder = ApplicationData.Current.LocalFolder;
                _recordingFile = await storageFolder.CreateFileAsync("CandidateInterview.mp4", CreationCollisionOption.ReplaceExisting);
                ViewModel.RecordingFilePath = _recordingFile.Path;

                var encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);

                await _mediaCapture.StartRecordToStorageFileAsync(encodingProfile, _recordingFile);
                _isRecording = true;

                System.Diagnostics.Debug.WriteLine($"Recording started: {_recordingFile.Path}");

                ViewModel.StartQuestions();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to start recording: {ex.Message}");
            }
        }

        private async void StopRecording_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (!_isRecording || _mediaCapture == null) return;

            try
            {
                StopVideoButton.IsEnabled = false;
                StartVideoButton.IsEnabled = true;
                SubmitVideoButton.IsEnabled = true;
                RecordingBorder.BorderThickness = new Thickness(0);
                NextQuestionButton.IsEnabled = false;

                await _mediaCapture.StopRecordAsync();
                _isRecording = false;

                System.Diagnostics.Debug.WriteLine("Recording stopped successfully.");

                ViewModel.ResetQuestions();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to stop recording: {ex.Message}");
            }
        }

        private void ExitPage_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            _mediaCapture?.Dispose();
            _mediaCapture = null;

            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

       
    }
}
