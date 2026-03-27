using Microsoft.UI.Xaml.Controls;
using Tests_and_Interviews.ViewModels;

namespace Tests_and_Interviews.Views
{

    public sealed partial class CandidateHomePage : Page
    {
        public CandidateViewModel ViewModel { get; }
        public CandidateHomePage()
        {
            InitializeComponent();
            ViewModel = new CandidateViewModel();
            this.DataContext = ViewModel;
        }
    }
}