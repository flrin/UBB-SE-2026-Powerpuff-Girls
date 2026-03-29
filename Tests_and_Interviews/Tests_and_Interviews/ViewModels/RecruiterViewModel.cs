using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Models.Enums;
using Tests_and_Interviews.Repositories;

namespace Tests_and_Interviews.ViewModels
{
    public class RecruiterViewModel : INotifyPropertyChanged
    {
        private readonly SlotRepository _slotRepo;
        private readonly InterviewSessionRepository _sessionRepo;

        private ObservableCollection<Slot> _slots = [];
        private DateTime _selectedDate = DateTime.Today;
        private ObservableCollection<InterviewSession> _pendingReviews = [];

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
                PendingReviews = [];
            }
        }

        public void LoadPendingReviews()
        {
            LoadPendingReviewsAsync();
        }

        public async Task LoadSlotsAsync()
        {

            int currentRecruiterId = 1;

            var existing = await _slotRepo.GetSlotsAsync(currentRecruiterId, SelectedDate.Date);
            var visibleSlots = new ObservableCollection<Slot>();

            var currentTime = SelectedDate.Date.AddHours(8);
            var endOfDay = SelectedDate.Date.AddHours(18);

            while (currentTime < endOfDay)
            {
                var overlappingSlot = existing.FirstOrDefault(s =>
                    s.StartTime < currentTime.AddMinutes(30) && s.EndTime > currentTime);

                if (overlappingSlot != null)
                {
                    visibleSlots.Add(overlappingSlot);
                    currentTime = overlappingSlot.EndTime;
                }
                else
                {
                    visibleSlots.Add(new Slot
                    {
                        RecruiterId = currentRecruiterId,
                        StartTime = currentTime,
                        EndTime = currentTime.AddMinutes(30),
                        Duration = 30,
                        Status = SlotStatus.Free,
                        InterviewType = string.Empty
                    });

                    currentTime = currentTime.AddMinutes(30);
                }
            }

            Slots = visibleSlots;
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