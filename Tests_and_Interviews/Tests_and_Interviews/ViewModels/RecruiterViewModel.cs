using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Repositories;
using Tests_and_Interviews.Helpers;

namespace Tests_and_Interviews.ViewModels
{
    public class RecruiterViewModel : INotifyPropertyChanged
    {
        private readonly SlotRepository _repo;

        private ObservableCollection<Slot> _slots = new();
        private DateTime _selectedDate = DateTime.Today;
        public ICommand CreateSlotCommand { get; }
        public event Action<Slot>? OnCreateSlotRequested;
        public event PropertyChangedEventHandler? PropertyChanged;

        public RecruiterViewModel()
        {
            _repo = new SlotRepository();
            CreateSlotCommand = new RelayCommand((obj) => CreateSlot((Slot)obj));
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
            var existingSlots = _repo.GetSlots(1, SelectedDate);
            var fullDay = new ObservableCollection<Slot>();
            var start = SelectedDate.Date.AddHours(8);
            var end = SelectedDate.Date.AddHours(18);

            int id = 1;
            while (start < end)
            {
                var slot = existingSlots.FirstOrDefault(s => s.StartTime == start);
                if (slot == null)
                {
                    fullDay.Add(new Slot
                    {
                        Id = id++,
                        RecruiterId = 1,
                        StartTime = start,
                        EndTime = start.AddHours(1),
                        Duration = 60,
                        Status = SlotStatus.Free,
                        InterviewType = "" 
                    });
                }
                else
                {
                    fullDay.Add(slot);
                }

                start = start.AddHours(1);
            }

            Slots = fullDay;
        }

        private void CreateSlot(Slot slot)
        {
            if (slot.Status == SlotStatus.Free)
            {
                OnCreateSlotRequested?.Invoke(slot);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}