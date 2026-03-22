using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using Tests_and_Interviews.ViewModels;

namespace Tests_and_Interviews.Views
{
    public sealed partial class TestPage : Page
    {
        public TestPageViewModel ViewModel { get; }

        public TestPage()
        {
            InitializeComponent();
            ViewModel = new TestPageViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is TestNavigationArgs args)
            {
                ViewModel.OnTimerExpired = async () =>
                {
                    await ViewModel.SubmitAsync();
                    ShowScoreDialog(0f, expired: true);
                };

                await ViewModel.LoadAsync(args.TestId, args.UserId);

                if (ViewModel.AlreadyAttempted)
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Test unavailable",
                        Content = "You have already attempted this test. Each test can only be taken once.",
                        CloseButtonText = "Back to Tests",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();
                    Frame.Navigate(typeof(MainTestPage));
                }
            }
        }

        private void BackToTests_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.StopTimer();
            Frame.Navigate(typeof(MainTestPage));
        }

        private async void SubmitTest_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Submit Test",
                Content = "Are you sure you want to submit? You cannot change your answers afterwards.",
                PrimaryButtonText = "Submit",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            float score = await ViewModel.SubmitAsync();
            ShowScoreDialog(score);
        }

        private void ShowScoreDialog(float score, bool expired = false)
        {
            var panel = new StackPanel { Spacing = 12 };

            if (expired)
                panel.Children.Add(new TextBlock
                {
                    Text = "? Time's up! Your test was automatically submitted.",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.Colors.OrangeRed)
                });

            panel.Children.Add(new TextBlock
            {
                Text = $"Your score: {score:F1} / 100",
                FontSize = 28,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.ColorHelper.FromArgb(255, 132, 148, 255))
            });

            var scoreDialog = new ContentDialog
            {
                Title = "Test Completed!",
                Content = panel,
                CloseButtonText = "Close",
                XamlRoot = this.XamlRoot
            };

            scoreDialog.CloseButtonClick += (s, e) =>
            {
                Frame.Navigate(typeof(MainTestPage));
            };

            _ = scoreDialog.ShowAsync();
        }
    }
}