using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Tests_and_Interviews.Services;

namespace Tests_and_Interviews
{
    public partial class App : Application
    {
        private Window? _window;

        public static int CurrentUserId { get; private set; } = 0;

        public App()
        {
            InitializeComponent();
            using (var db = new AppDbContext())
            {
                db.SeedDatabase();

                var alice = db.Users.FirstOrDefault(u => u.Name == "Alice Johnson");
                CurrentUserId = alice?.Id ?? 0;
                System.Diagnostics.Debug.WriteLine($"[App] CurrentUserId = {CurrentUserId}");
            }
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
        }
    }
}