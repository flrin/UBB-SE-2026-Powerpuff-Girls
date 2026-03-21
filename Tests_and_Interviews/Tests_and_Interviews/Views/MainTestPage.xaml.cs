using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using Tests_and_Interviews.ViewModels;

namespace Tests_and_Interviews.Views
{
    public sealed partial class MainTestPage : Page
    {
        public MainTestViewModel ViewModel { get; }

        public MainTestPage()
        {
            InitializeComponent();
            ViewModel = new MainTestViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadTestsAsync();
        }

        private void StartTest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int testId = Convert.ToInt32(btn.Tag);
                Frame.Navigate(typeof(TestPage), new TestNavigationArgs
                {
                    TestId = testId,
                    UserId = 1
                });
            }
        }
    }
}