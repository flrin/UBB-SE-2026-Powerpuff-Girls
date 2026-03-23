using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.ViewModels;

namespace Tests_and_Interviews.Views
{
    public sealed partial class RecruiterPage : Page
    {
        public RecruiterPage()
        {
            this.InitializeComponent();

            if (DataContext is RecruiterViewModel vm)
            {
                vm.OnCreateSlotRequested += ShowCreateSlotDialog;
            }
        }

        private RecruiterViewModel ViewModel => (RecruiterViewModel)DataContext;

        private void MainCalendar_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
        {
            if (sender.SelectedDates.Count > 0)
            {
                ViewModel.SelectedDate = sender.SelectedDates[0].DateTime;
            }
        }

        private async void ShowCreateSlotDialog(Slot slot)
        {
            var combo = new ComboBox
            {
                ItemsSource = new List<string> { "60 min", "90 min" },
                SelectedIndex = 1
            };
            var dialog = new ContentDialog
            {
                Title = "Create Slot",
                Content = new StackPanel
                {
                    Spacing = 10,
                    Children =
                    {
                        new TextBlock { Text = $"Date: {slot.StartTime:dd/MM/yyyy}" },
                        new TextBlock { Text = $"Time: {slot.StartTime:HH:mm} - {slot.EndTime:HH:mm}" },
                        combo
                    }
                },
                PrimaryButtonText = "Make Available",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync().AsTask(); 
            if (result == ContentDialogResult.Primary)
            {
                slot.Status = SlotStatus.Booked; 
                slot.InterviewType = "Available";

                ViewModel.LoadSlots();
            }
        }
    }
}