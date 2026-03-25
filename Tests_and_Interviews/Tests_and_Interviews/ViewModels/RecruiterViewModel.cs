using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Repositories;

namespace Tests_and_Interviews.ViewModels
{
    public class RecruiterViewModel : INotifyPropertyChanged
    {
        private readonly SlotRepository _repo;
        private ObservableCollection<Slot> _slots = new ObservableCollection<Slot>();
        private DateTime _selectedDate = DateTime.Today;

        public event PropertyChangedEventHandler? PropertyChanged;

        public RecruiterViewModel()
        {
            _repo = new SlotRepository();
            LoadSlots();
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedDateFormatted));
                LoadSlots();
            }
        }

        public string SelectedDateFormatted =>
            SelectedDate.ToString("dddd dd/MM/yyyy");

        public ObservableCollection<Slot> Slots
        {
            get => _slots;
            set
            {
                _slots = value;
                OnPropertyChanged();
            }
        }

        public void LoadSlots()
        {
            var existing = _repo.GetSlots(1, SelectedDate.Date);
            var fullDay = new ObservableCollection<Slot>();

            Slots = new ObservableCollection<Slot>(
                existing.OrderBy(s => s.StartTime)
            );
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}