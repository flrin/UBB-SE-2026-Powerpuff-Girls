using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Tests_and_Interviews.Services;
using Tests_and_Interviews.Helpers;
using System.Windows.Input;

namespace Tests_and_Interviews.ViewModels
{
    public class TestViewModel : INotifyPropertyChanged
    {
        private TestService _testService;
        private string _displayText = "Waiting for data";
        public ICommand LoadDataCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public TestViewModel() 
        {
            _testService = new TestService();

            LoadDataCommand = new RelayCommand(LoadData);
        }

        public string DisplayText
        {
            get { return _displayText; }
            set 
            {
                if (_displayText != value)
                {
                    _displayText = value;

                    OnPropertyChanged();
                }
            }
        }

        private void LoadData()
        {
            DisplayText = _testService.get_test();
        }

    }
}
