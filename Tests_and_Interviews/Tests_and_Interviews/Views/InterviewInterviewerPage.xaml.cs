using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using Tests_and_Interviews.ViewModels;
using Windows.Globalization.NumberFormatting;
using Windows.Media.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Tests_and_Interviews.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InterviewInterviewerPage : Page
    {

        public InterviewInterviewerViewModel ViewModel { get; set; }

        public InterviewInterviewerPage()
        {
            InitializeComponent();
            ViewModel = new InterviewInterviewerViewModel();
            SetNumberBoxNumberFormatter();
            this.DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is int id && id > 0)
            {
                ViewModel.InitializeSession(id);
            }
        }

        private void SubmitScore_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {

            ViewModel.SubmitScore();


            if (this.Tag is Window hostWindow)
            {
                try
                {
                    hostWindow.Close();
                    return;
                }
                catch { }
            }

            if (this.Frame != null && this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void Skip10_Click(object sender, RoutedEventArgs e)
        {
            if (InterviewPlayer.MediaPlayer == null) return;

            var session = InterviewPlayer.MediaPlayer.PlaybackSession;
            session.Position += TimeSpan.FromSeconds(10);
        }

        public MediaSource WidegetMediaSource(Uri uri)
        {
            return uri != null ? MediaSource.CreateFromUri(uri) : null;
        }

        private void SetNumberBoxNumberFormatter()
        {
            IncrementNumberRounder rounder = new IncrementNumberRounder
            {
                Increment = 0.01,
                RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp
            };

            DecimalFormatter formatter = new DecimalFormatter
            {
                IntegerDigits = 1,
                FractionDigits = 2,
                NumberRounder = rounder
            };
            FormattedNumberBox.NumberFormatter = formatter;
            FormattedNumberBox.Minimum = 1;
            FormattedNumberBox.Maximum = 10;
        }
    }
}
