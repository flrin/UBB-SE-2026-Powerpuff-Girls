using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
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

                var selected = ViewModel.Tests.FirstOrDefault(t => t.TestId == testId);
                if (selected != null) ViewModel.SelectedTest = selected;

                Frame.Navigate(typeof(TestPage), new TestNavigationArgs
                {
                    TestId = testId,
                    UserId = App.CurrentUserId
                });
            }
        }

        private void Card_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int testId = Convert.ToInt32(btn.Tag);
                var card = ViewModel.Tests.FirstOrDefault(t => t.TestId == testId);
                if (card != null) card.IsHovered = true;
            }
        }

        private void Card_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int testId = Convert.ToInt32(btn.Tag);
                var card = ViewModel.Tests.FirstOrDefault(t => t.TestId == testId);
                if (card != null) card.IsHovered = false;
            }
        }
    }
}