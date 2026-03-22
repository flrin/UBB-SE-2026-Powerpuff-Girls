using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
    }
}