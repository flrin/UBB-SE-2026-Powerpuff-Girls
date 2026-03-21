using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Repositories;
using Tests_and_Interviews.Services;

namespace Tests_and_Interviews.ViewModels
{
    public class TestCardViewModel
    {
        public int TestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string QuestionTypeLabel { get; set; } = string.Empty;
    }

    public class MainTestViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(NoTestsVisible)); }
        }

        public ObservableCollection<TestCardViewModel> Tests { get; } = new();

        public Visibility NoTestsVisible =>
            (!IsLoading && Tests.Count == 0) ? Visibility.Visible : Visibility.Collapsed;

        private readonly AppDbContext _db;

        public MainTestViewModel()
        {
            _db = new AppDbContext();
        }

        public async Task LoadTestsAsync()
        {
            IsLoading = true;
            Tests.Clear();

            var repo = new TestRepository(_db);

            
            var categories = new List<string> { "Programming", "Database" };

            foreach (var cat in categories)
            {
                var tests = await repo.FindTestsByCategoryAsync(cat);
                foreach (var t in tests)
                {
                    string typeLabel = "MIXED";
                    if (t.Questions.Count > 0)
                        typeLabel = t.Questions[0].QuestionTypeString.Replace("_", "/");

                    Tests.Add(new TestCardViewModel
                    {
                        TestId = t.Id,
                        Title = t.Title,
                        Category = t.Category,
                        QuestionTypeLabel = typeLabel
                    });
                }
            }

            IsLoading = false;
            OnPropertyChanged(nameof(NoTestsVisible));
        }
    }
}