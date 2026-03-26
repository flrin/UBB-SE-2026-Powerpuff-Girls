using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
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
        private readonly SlotRepository _repo;
        private ObservableCollection<Slot> _slots = new ObservableCollection<Slot>();
        private DateTime _selectedDate = DateTime.Today;
        private ObservableCollection<InterviewSession> _pendingReviews = new ObservableCollection<InterviewSession>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public RecruiterViewModel()
        {
            _repo = new SlotRepository();
            LoadSlots();
            LoadPendingReviews();
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

        public ObservableCollection<InterviewSession> PendingReviews
        {
            get => _pendingReviews;
            set
            {
                _pendingReviews = value;
                OnPropertyChanged();
            }
        }

        public void LoadPendingReviews()
        {
            try
            {
                using var db = new AppDbContext();
                var list = db.InterviewSessions
                    .Where(s => s.Status == InterviewStatus.InProgress.ToString())
                    .OrderByDescending(s => s.DateStart)
                    .ToList();

                PendingReviews = new ObservableCollection<InterviewSession>(list);
            }
            catch
            {
                PendingReviews = new ObservableCollection<InterviewSession>();
            }
        }

        public void LoadSlots()
        {
            var existing = _repo.GetSlots(1, SelectedDate.Date);
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

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}