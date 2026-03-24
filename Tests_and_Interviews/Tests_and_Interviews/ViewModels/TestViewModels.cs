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
    public class TestCardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public int TestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string QuestionTypeLabel { get; set; } = string.Empty;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CardBorderThickness));
                OnPropertyChanged(nameof(CardBorderBrush));
            }
        }

        private bool _isHovered;
        public bool IsHovered
        {
            get => _isHovered;
            set
            {
                _isHovered = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CardBorderThickness));
                OnPropertyChanged(nameof(CardBorderBrush));
            }
        }

        public Microsoft.UI.Xaml.Thickness CardBorderThickness =>
            IsSelected || IsHovered
                ? new Microsoft.UI.Xaml.Thickness(2.5)
                : new Microsoft.UI.Xaml.Thickness(1);

        public Microsoft.UI.Xaml.Media.SolidColorBrush CardBorderBrush =>
            IsSelected
                ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 132, 148, 255))
                : IsHovered
                    ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 30, 30, 30))
                    : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 232, 228, 255));
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

        private TestCardViewModel? _selectedTest;
        public TestCardViewModel? SelectedTest
        {
            get => _selectedTest;
            set
            {
                if (_selectedTest != null) _selectedTest.IsSelected = false;
                _selectedTest = value;
                if (_selectedTest != null) _selectedTest.IsSelected = true;
                OnPropertyChanged();
            }
        }

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

            var categories = new List<string> { "Programming", "Database", "Computer Science" };

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