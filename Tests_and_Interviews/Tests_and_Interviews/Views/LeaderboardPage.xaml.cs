using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Repositories;
using Tests_and_Interviews.Services;

namespace Tests_and_Interviews.Views
{
    public sealed partial class LeaderboardPage : Page
    {
        private List<LeaderboardEntry> _entries = new();
        private int _currentPage = 1;
        private const int PageSize = 10;
        private int _testId;

        public LeaderboardPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is int testId)
            {
                _testId = testId;

                var repo = new LeaderboardRepository();
                var service = new LeaderboardService();

                _entries = await service.GetFullLeaderboardAsync(testId);
                RenderPage();
            }
        }

        private void RenderPage()
        {
            LeaderboardPanel.Children.Clear();

            var pagedEntries = _entries
                .Skip((_currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            foreach (var entry in pagedEntries)
            {
                bool isCurrentUser = entry.UserId == App.CurrentUserId;

                var border = new Border
                {
                    Background = new SolidColorBrush(
                        isCurrentUser
                            ? Microsoft.UI.ColorHelper.FromArgb(255, 238, 234, 255)
                            : Microsoft.UI.Colors.White),
                    BorderBrush = new SolidColorBrush(
                        isCurrentUser
                            ? Microsoft.UI.ColorHelper.FromArgb(255, 132, 148, 255)
                            : Microsoft.UI.ColorHelper.FromArgb(255, 232, 228, 255)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(20)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

                var rankText = new TextBlock
                {
                    Text = $"#{entry.RankPosition}",
                    FontSize = 18,
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    Foreground = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 132, 148, 255)),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(rankText, 0);

                var nameText = new TextBlock
                {
                    Text = entry.User?.Name ?? "Unknown user",
                    FontSize = 16,
                    FontWeight = isCurrentUser
                        ? Microsoft.UI.Text.FontWeights.SemiBold
                        : Microsoft.UI.Text.FontWeights.Normal,
                    Foreground = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 26, 26, 46)),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(nameText, 1);

                var scoreText = new TextBlock
                {
                    Text = $"{entry.NormalizedScore:F1}%",
                    FontSize = 16,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 26, 26, 46)),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(scoreText, 2);

                grid.Children.Add(rankText);
                grid.Children.Add(nameText);
                grid.Children.Add(scoreText);

                border.Child = grid;
                LeaderboardPanel.Children.Add(border);
            }

            int totalPages = Math.Max(1, (int)Math.Ceiling((double)_entries.Count / PageSize));
            PageInfoText.Text = $"Page {_currentPage} of {totalPages}";
            PrevButton.IsEnabled = _currentPage > 1;
            NextButton.IsEnabled = _currentPage < totalPages;
        }

        private void BackToTests_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainTestPage));
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                RenderPage();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)_entries.Count / PageSize));
            if (_currentPage < totalPages)
            {
                _currentPage++;
                RenderPage();
            }
        }
    }
}