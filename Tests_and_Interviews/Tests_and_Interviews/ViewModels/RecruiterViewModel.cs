using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Repositories;
using Tests_and_Interviews.Services;
using Tests_and_Interviews.Helpers;
using Tests_and_Interviews.Models.Enums;

namespace Tests_and_Interviews.ViewModels
{
    public class RecruiterViewModel : INotifyPropertyChanged
    {
        private readonly SlotRepository _slotRepo;
        private readonly InterviewSessionRepository _sessionRepo;

        private ObservableCollection<Slot> _slots = new ObservableCollection<Slot>();
        private DateTime _selectedDate = DateTime.Today;
        private ObservableCollection<InterviewSession> _pendingReviews = new ObservableCollection<InterviewSession>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public RecruiterViewModel()
        {
            _slotRepo = new SlotRepository();
            _sessionRepo = new InterviewSessionRepository();

            _ = InitializeDataAsync();
        }

        private async Task InitializeDataAsync()
        {
            await LoadSlotsAsync();
            await LoadPendingReviewsAsync();
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedDateFormatted));

                    _ = LoadSlotsAsync();
                }
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

        public ObservableCollection<InterviewSession> PendingReviews
        {
            get => _pendingReviews;
            set
            {
                _pendingReviews = value;
                OnPropertyChanged();
            }
        }

        public async Task LoadPendingReviewsAsync()
        {
            try
            {
                var list = await _sessionRepo.GetSessionsByStatusAsync(InterviewStatus.InProgress.ToString());
                PendingReviews = new ObservableCollection<InterviewSession>(list);
            }
            catch
            {
                PendingReviews = new ObservableCollection<InterviewSession>();
            }
        }

        public void LoadPendingReviews()
        {
            LoadPendingReviewsAsync();
        }

        public async Task LoadSlotsAsync()
        {
            var existing = await _slotRepo.GetSlotsAsync(1, SelectedDate.Date);
            var fullDay = new ObservableCollection<Slot>();

            var start = SelectedDate.Date.AddHours(8);
            var end = SelectedDate.Date.AddHours(18);

            while (start < end)
            {
                var slot = existing.FirstOrDefault(s =>
                    start >= s.StartTime && start < s.EndTime);

                if (slot != null)
                {
                    bool isStart = start == slot.StartTime;

                    fullDay.Add(new Slot
                    {
                        StartTime = start,
                        EndTime = slot.EndTime,
                        Duration = slot.Duration,
                        Status = slot.Status,
                        InterviewType = slot.InterviewType,
                        IsHidden = !isStart
                    });
                }
                else
                {
                    fullDay.Add(new Slot
                    {
                        StartTime = start,
                        EndTime = start.AddMinutes(30),
                        Duration = 30,
                        Status = SlotStatus.Free,
                        InterviewType = "",
                        IsHidden = false
                    });
                }

                start = start.AddMinutes(30);
            }

            Slots = new ObservableCollection<Slot>(fullDay.Where(s => !s.IsHidden));
        }

        public void LoadSlots()
        {
            LoadSlotsAsync();
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}