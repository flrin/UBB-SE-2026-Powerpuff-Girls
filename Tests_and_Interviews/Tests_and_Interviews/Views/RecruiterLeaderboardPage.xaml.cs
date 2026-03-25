using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Models.Enums;
using Tests_and_Interviews.Services;

namespace Tests_and_Interviews.Views
{
    public sealed partial class RecruiterLeaderboardPage : Page
    {
        private List<TestAttempt> _entries = new();
        private int _currentPage = 1;
        private const int PageSize = 10;
        private int _testId;

        public RecruiterLeaderboardPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is int testId)
            {
                _testId = testId;
            }

            using var db = new AppDbContext();

            var test = db.Tests.FirstOrDefault(t => t.Id == _testId);
            if (test != null)
            {
                PageTitleText.Text = test.Title;
                PageSubtitleText.Text = "Detailed recruiter leaderboard view";
            }

            _entries = db.TestAttempts
                .Include(a => a.User)
                .Include(a => a.Test)
                .Include(a => a.Answers)
                    .ThenInclude(ans => ans.Question)
                .Where(a =>
                    a.TestId == _testId &&
                    a.Status == TestStatus.COMPLETED.ToString() &&
                    a.IsValidated &&
                    a.PercentageScore != null &&
                    a.CompletedAt != null)
                .OrderByDescending(a => a.PercentageScore)
                .ThenBy(a => a.CompletedAt)
                .ToList();

            RenderPage();
        }

        private void RenderPage()
        {
            LeaderboardPanel.Children.Clear();

            var pagedEntries = _entries
                .Skip((_currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            int rankBase = (_currentPage - 1) * PageSize;

            for (int i = 0; i < pagedEntries.Count; i++)
            {
                var entry = pagedEntries[i];
                int rank = rankBase + i + 1;

                var border = new Border
                {
                    Background = new SolidColorBrush(Microsoft.UI.Colors.White),
                    BorderBrush = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 232, 228, 255)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(20)
                };

                var panel = new StackPanel { Spacing = 8 };

                var topGrid = new Grid();
                topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
                topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

                var rankText = new TextBlock
                {
                    Text = $"#{rank}",
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
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 26, 26, 46)),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(nameText, 1);

                var scoreText = new TextBlock
                {
                    Text = $"{entry.PercentageScore:F1}%",
                    FontSize = 16,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 26, 26, 46)),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(scoreText, 2);

                topGrid.Children.Add(rankText);
                topGrid.Children.Add(nameText);
                topGrid.Children.Add(scoreText);
                int durationMinutes = GetDurationMinutes(entry);

                var rawScoreText = new TextBlock
                {
                    Text = $"Raw score: {entry.Score:F1} / 100",
                    FontSize = 13,
                    Foreground = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 110, 110, 110)),
                    TextWrapping = TextWrapping.Wrap
                };

                var startedCompletedText = new TextBlock
                {
                    Text = $"Started at: {entry.StartedAt:dd/MM/yyyy HH:mm}    |    Completed at: {entry.CompletedAt:dd/MM/yyyy HH:mm}",
                    FontSize = 13,
                    Foreground = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 110, 110, 110)),
                    TextWrapping = TextWrapping.Wrap
                };

                var durationText = new TextBlock
                {
                    Text = $"Duration: {durationMinutes} min",
                    FontSize = 13,
                    Foreground = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 110, 110, 110)),
                    TextWrapping = TextWrapping.Wrap
                };

                panel.Children.Add(topGrid);
                panel.Children.Add(rawScoreText);
                panel.Children.Add(startedCompletedText);
                panel.Children.Add(durationText);

                border.Child = panel;
                LeaderboardPanel.Children.Add(border);
            }

            int totalPages = Math.Max(1, (int)Math.Ceiling((double)_entries.Count / PageSize));
            PageInfoText.Text = $"Page {_currentPage} of {totalPages}";
            PrevButton.IsEnabled = _currentPage > 1;
            NextButton.IsEnabled = _currentPage < totalPages;
        }


        private bool AreMultipleChoiceAnswersEqual(string submitted, string correct)
        {
            var submittedSet = ParseAnswerIndexes(submitted);
            var correctSet = ParseAnswerIndexes(correct);

            return submittedSet.SetEquals(correctSet);
        }

        private HashSet<int> ParseAnswerIndexes(string value)
        {
            var result = new HashSet<int>();

            if (string.IsNullOrWhiteSpace(value))
                return result;

            var cleaned = value.Trim().TrimStart('[').TrimEnd(']');
            if (string.IsNullOrWhiteSpace(cleaned))
                return result;

            foreach (var part in cleaned.Split(','))
            {
                if (int.TryParse(part.Trim(), out int index))
                    result.Add(index);
            }

            return result;
        }

        private int GetDurationMinutes(TestAttempt attempt)
        {
            if (attempt.CompletedAt == null)
                return 0;

            return (int)(attempt.CompletedAt.Value - attempt.StartedAt).TotalMinutes;
        }

        private void BackToRecruiterTests_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RecruiterTestsPage));
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