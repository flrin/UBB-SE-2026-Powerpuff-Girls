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
            // Note: I replaced the hardcoded '1' with a generic property or parameter. 
            // Adjust RecruiterId as needed for your actual ViewModel state.
            int currentRecruiterId = 1;

            var existing = await _slotRepo.GetSlotsAsync(currentRecruiterId, SelectedDate.Date);
            var visibleSlots = new ObservableCollection<Slot>();

            var currentTime = SelectedDate.Date.AddHours(8);
            var endOfDay = SelectedDate.Date.AddHours(18);

            while (currentTime < endOfDay)
            {
                // Check if any existing slot overlaps with the CURRENT 30-minute block we are looking at
                var overlappingSlot = existing.FirstOrDefault(s =>
                    s.StartTime < currentTime.AddMinutes(30) && s.EndTime > currentTime);

                if (overlappingSlot != null)
                {
                    // We found an occupied slot! Add the REAL object from the database directly.
                    // This preserves the Id, RecruiterId, and all other database properties.
                    visibleSlots.Add(overlappingSlot);

                    // FAST-FORWARD the clock to the end of this slot. 
                    // If it's a 90-minute slot, this safely jumps us over the next two 30-minute ticks.
                    currentTime = overlappingSlot.EndTime;
                }
                else
                {
                    // No slot exists here. Create a 30-minute Free block.
                    visibleSlots.Add(new Slot
                    {
                        RecruiterId = currentRecruiterId,
                        StartTime = currentTime,
                        EndTime = currentTime.AddMinutes(30),
                        Duration = 30,
                        Status = SlotStatus.Free,
                        InterviewType = string.Empty
                    });

                    // Tick the clock forward by 30 minutes to check the next block
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