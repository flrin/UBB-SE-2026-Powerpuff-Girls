using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tests_and_Interviews.Repositories;
using Tests_and_Interviews.Views;

namespace Tests_and_Interviews
{
    public partial class App : Application
    {
        private Window? _window;

        public static int CurrentUserId { get; private set; } = 0;

        private readonly string _connectionString = "Server=localhost;Database=WinUIDevDb;User Id=devuser;Password=devpassword;TrustServerCertificate=True;";

        public App()
        {
            InitializeComponent();

            var userRepo = new UserRepository();

            try
            {
                var users = Task.Run(() => userRepo.GetAllAsync()).Result;
                var alice = users.FirstOrDefault(u => u.Name == "Alice Johnson");

                CurrentUserId = alice?.Id ?? 0;
                System.Diagnostics.Debug.WriteLine($"[App] CurrentUserId = {CurrentUserId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Failed to fetch users from database. Did you run the SQL seed script? Error: {ex.Message}");
                CurrentUserId = 0;
            }
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Main test window
            _window = new MainWindow();
            _window.Activate();

            // Recruiter window
            var recruiterWindow = new Window();
            var recruiterFrame = new Frame();
            recruiterFrame.Navigate(typeof(RecruiterPage));
            recruiterWindow.Content = recruiterFrame;
            recruiterWindow.Title = "Recruiter";
            recruiterWindow.Activate();

            // Candidate home window
            var candidateWindow = new Window();
            var candidateFrame = new Frame();
            candidateFrame.Navigate(typeof(CandidateHomePage));
            candidateWindow.Content = candidateFrame;
            candidateWindow.Title = "Candidate Home";
            candidateWindow.Activate();
        }
    }
}