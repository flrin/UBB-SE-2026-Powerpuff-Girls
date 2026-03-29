using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
using Tests_and_Interviews.Repositories;
using Tests_and_Interviews.Services;
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
                    Text = "Time's up! Your test was automatically submitted.",
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

            scoreDialog.CloseButtonClick += async (s, e) =>
            {
                await ShowSummaryLeaderboardDialogAsync();
            };

            _ = scoreDialog.ShowAsync();
        }

        private async System.Threading.Tasks.Task ShowSummaryLeaderboardDialogAsync()
        {
            var leaderboardRepo = new LeaderboardRepository();
            var leaderboardService = new LeaderboardService();

            var topThree = await leaderboardService.GetTopThreeAsync(ViewModel.TestId);
            var currentUserEntry = await leaderboardService.GetUserRankingAsync(App.CurrentUserId, ViewModel.TestId);

            var panel = new StackPanel { Spacing = 10 };

            panel.Children.Add(new TextBlock
            {
                Text = "Top 3 for this test",
                FontSize = 20,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.ColorHelper.FromArgb(255, 26, 26, 46))
            });

            foreach (var entry in topThree)
            {
                panel.Children.Add(
                    CreateSummaryEntryCard(
                        entry.RankPosition,
                        entry.User?.Name ?? "Unknown user",
                        entry.NormalizedScore,
                        entry.UserId == App.CurrentUserId
                    )
                );
            }

            bool currentUserInTopThree = currentUserEntry != null &&
                                         topThree.Any(e => e.UserId == currentUserEntry.UserId);

            if (currentUserEntry != null && !currentUserInTopThree)
            {
                panel.Children.Add(new Border
                {
                    Height = 1,
                    Margin = new Thickness(0, 8, 0, 8),
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 232, 228, 255))
                });

                panel.Children.Add(new TextBlock
                {
                    Text = "Your position",
                    FontSize = 16,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Margin = new Thickness(0, 10, 0, 6)
                });

                panel.Children.Add(
                    CreateSummaryEntryCard(
                        currentUserEntry.RankPosition,
                        currentUserEntry.User?.Name ?? "You",
                        currentUserEntry.NormalizedScore,
                        true
                    )
                );
            }

            var dialog = new ContentDialog
            {
                Title = "Summary Leaderboard",
                Content = panel,
                PrimaryButtonText = "See Full Leaderboard",
                CloseButtonText = "Close",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Frame.Navigate(typeof(LeaderboardPage), ViewModel.TestId);
            }
            else
            {
                Frame.Navigate(typeof(MainTestPage));
            }
        }

        private Border CreateSummaryEntryCard(int rank, string name, decimal score, bool isCurrentUser = false)
        {
            var border = new Border
            {
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    isCurrentUser
                        ? Microsoft.UI.ColorHelper.FromArgb(255, 238, 234, 255)
                        : Microsoft.UI.Colors.White),
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    isCurrentUser
                        ? Microsoft.UI.ColorHelper.FromArgb(255, 132, 148, 255)
                        : Microsoft.UI.ColorHelper.FromArgb(255, 232, 228, 255)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 8)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });

            var rankText = new TextBlock
            {
                Text = rank == 1 ? "🥇"
                     : rank == 2 ? "🥈"
                     : rank == 3 ? "🥉"
                     : $"#{rank}",
                FontSize = rank <= 3 ? 22 : 18,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.ColorHelper.FromArgb(255, 132, 148, 255)),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(rankText, 0);

            var nameText = new TextBlock
            {
                Text = name,
                FontSize = 15,
                FontWeight = isCurrentUser
                    ? Microsoft.UI.Text.FontWeights.SemiBold
                    : Microsoft.UI.Text.FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetColumn(nameText, 1);

            var scoreText = new TextBlock
            {
                Text = $"{score:F1}%",
                FontSize = 15,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(scoreText, 2);

            grid.Children.Add(rankText);
            grid.Children.Add(nameText);
            grid.Children.Add(scoreText);

            border.Child = grid;
            return border;
        }
    }
}