using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Repositories;
using Tests_and_Interviews.ViewModels;

namespace Tests_and_Interviews.Views
{
    public sealed partial class RecruiterPage : Page
    {
        public RecruiterPage()
        {
            this.InitializeComponent();
        }

        private RecruiterViewModel ViewModel => (RecruiterViewModel)DataContext;

        private void MainCalendar_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
        {
            if (sender.SelectedDates.Count > 0)
            {
                ViewModel.SelectedDate = sender.SelectedDates[0].DateTime;
            }
        }

        private async void Slot_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (sender is Grid grid && grid.DataContext is Slot slot)
            {
                if (slot.Status != SlotStatus.Free || slot.InterviewType == "Available")
                    return;

                var combo = new ComboBox
                {
                    ItemsSource = new List<string> { "60 min", "90 min" },
                    SelectedIndex = 0
                };

                var dialog = new ContentDialog
                {
                    Title = "Create Slot",
                    Content = combo,
                    PrimaryButtonText = "Create",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };

                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    int duration = combo.SelectedIndex == 0 ? 60 : 90;

                    var repo = new SlotRepository();
                    repo.Add(new Slot
                    {
                        RecruiterId = 1,
                        StartTime = slot.StartTime,
                        EndTime = slot.StartTime.AddMinutes(duration),
                        Duration = duration,
                        Status = SlotStatus.Free, 
                        InterviewType = "Available"
                    });

                    ViewModel.LoadSlots();
                }
            }
        }

        private void LeaderboardInfo_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RecruiterTestsPage));
        }

        private void RefreshPendingReviews_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadPendingReviews();
        }

        private void ReviewPending_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                var tag = b.Tag;
                int sessionId = 0;
                if (tag is int i) sessionId = i;
                else if (tag is string s && int.TryParse(s, out int parsed)) sessionId = parsed;

                if (sessionId > 0)
                {
                    Frame.Navigate(typeof(InterviewInterviewerPage), sessionId);
                }
            }
        }
    }
}