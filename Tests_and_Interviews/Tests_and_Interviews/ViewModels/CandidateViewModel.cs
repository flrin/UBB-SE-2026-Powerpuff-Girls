using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Tests_and_Interviews.Helpers;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Repositories;
using Tests_and_Interviews.Services;
using Tests_and_Interviews.Views;

namespace Tests_and_Interviews.ViewModels
{
    public class CandidateViewModel : INotifyPropertyChanged
    {
        private readonly BookingService _bookingService;
        private readonly InterviewSessionRepository _interviewSessionRepo;
        private readonly NotificationService _notificationService;
        private readonly SlotRepository _slotRepository;

        private List<Slot> _availableSlots;
        private List<Slot> _availableDays;
        private ObservableCollection<Company> _matchedCompanies;
        private Company _selectedCompany;
        private Slot _selectedSlot;
        private DateTime _selectedDay;
        private int _dayStartIndex = 0;
        private bool _isBookingVisible;
        private ObservableCollection<InterviewSession> _interviewSessions;

        public ICommand LoadSlotsCommand { get; }
        public ICommand ScheduleCommand { get; }
        public ICommand SelectDayCommand { get; }
        public ICommand SelectSlotCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand NextDaysCommand { get; }
        public ICommand PrevDaysCommand { get; }
        public ICommand JoinCommand { get; }
        public ICommand CancelCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public CandidateViewModel()
        {
            _bookingService = new BookingService();
            _interviewSessionRepo = new InterviewSessionRepository();
            _notificationService = new NotificationService();
            _slotRepository = new SlotRepository();

            LoadSlotsCommand = new RelayCommand(_ => LoadSlots());
            ScheduleCommand = new RelayCommand((obj) => Schedule((Company)obj));
            JoinCommand = new RelayCommand((obj) => Join(obj));
            CancelCommand = new RelayCommand(Cancel);

            SelectDayCommand = new RelayCommand((obj) =>
            {
                var selected = (Slot)obj;
                if (AvailableDays == null) return;

                foreach (var d in AvailableDays)
                    d.IsDaySelected = false;

                selected.IsDaySelected = true;
                SelectedDay = selected.StartTime.Date;
                SelectedSlot = null;
                OnPropertyChanged(nameof(AvailableDays));
            });

            SelectSlotCommand = new RelayCommand((obj) => SelectSlot(obj));

            ConfirmCommand = new RelayCommand(ConfirmBooking);

            NextDaysCommand = new RelayCommand(_ =>
            {
                if (_dayStartIndex + 3 < (AvailableDays?.Count ?? 0))
                {
                    _dayStartIndex++;
                    OnPropertyChanged(nameof(VisibleDays));
                }
            });

            PrevDaysCommand = new RelayCommand(_ =>
            {
                if (_dayStartIndex > 0)
                {
                    _dayStartIndex--;
                    OnPropertyChanged(nameof(VisibleDays));
                }
            });

            MatchedCompanies =
            [
                new Company { CompanyName = "Google", JobTitle = "Frontend Dev", RecruiterId = 1 },
                new Company { CompanyName = "Amazon", JobTitle = "Backend Dev", RecruiterId = 2 }
            ];

            _ = LoadInterviewSessionsAsync();
        }

        public ObservableCollection<InterviewSession> InterviewSessions
        {
            get => _interviewSessions;
            set
            {
                if (_interviewSessions != value)
                {
                    _interviewSessions = value;
                    OnPropertyChanged();
                }
            }
        }

        private void SelectSlot(Object obj)
        {
            var selected = (Slot)obj;
            if (AvailableSlots == null) return;

            foreach (var s in AvailableSlots)
                s.IsSlotSelected = false;

            selected.IsSlotSelected = true;

            SelectedSlot = selected;
            OnPropertyChanged(nameof(AvailableSlots));
        }

        public async Task LoadInterviewSessionsAsync()
        {
            InterviewSessions = [];
            try
            {
                var sessions = await _interviewSessionRepo.GetScheduledSessionsAsync();

                foreach (var s in sessions)
                {
                    InterviewSessions.Add(s);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load sessions: {ex.Message}");
            }
        }

        public List<Slot> AvailableSlots
        {
            get => _availableSlots;
            set
            {
                if (_availableSlots != value)
                {
                    _availableSlots = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<Slot> AvailableDays
        {
            get => _availableDays;
            set
            {
                if (_availableDays != value)
                {
                    _availableDays = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(VisibleDays));
                }
            }
        }

        public IEnumerable<Slot> VisibleDays => (AvailableDays ?? [])
            .Skip(_dayStartIndex)
            .Take(3)
            .ToList();

        public ObservableCollection<Company> MatchedCompanies
        {
            get => _matchedCompanies;
            set
            {
                if (_matchedCompanies != value)
                {
                    _matchedCompanies = value;
                    OnPropertyChanged();
                }
            }
        }

        public Company SelectedCompany
        {
            get => _selectedCompany;
            set { _selectedCompany = value; OnPropertyChanged(); }
        }

        public Slot SelectedSlot
        {
            get => _selectedSlot;
            set
            {
                _selectedSlot = value; OnPropertyChanged();

            }
        }

        public DateTime SelectedDay
        {
            get => _selectedDay;
            set { _selectedDay = value; OnPropertyChanged(); LoadSlotsForSelectedDay(); }
        }

        public bool IsBookingVisible
        {
            get => _isBookingVisible;
            set { _isBookingVisible = value; OnPropertyChanged(); }
        }

        private void LoadSlots()
        {
            MatchedCompanies =
            [
                new Company { CompanyName = "Google", JobTitle = "Frontend Dev", RecruiterId = 1 },
                new Company { CompanyName = "Amazon", JobTitle = "Backend Dev", RecruiterId = 2 }
            ];
        }

        private void Schedule(Company company)
        {
            IsBookingVisible = true;
            SelectedCompany = company;

            var slots = _bookingService.GetAvailableSlotsByRecruiterId(company.RecruiterId);

            AvailableDays = slots
                .GroupBy(s => s.StartTime.Date)
                .Select(g => g.First())
                .ToList();

            SelectedDay = AvailableDays.FirstOrDefault()?.StartTime.Date ?? DateTime.Today;
        }

        private void LoadSlotsForSelectedDay()
        {
            if (SelectedCompany == null) return;

            AvailableSlots = _bookingService
                .GetAvailableSlotsByRecruiterId(SelectedCompany.RecruiterId)
                .Where(s => s.StartTime.Date == SelectedDay.Date)
                .ToList();
        }

        private async void ConfirmBooking(object obj)
        {
            if (SelectedSlot == null)
                return;

            _bookingService.ConfirmBooking(1, SelectedSlot);

            await LoadInterviewSessionsAsync();

            try
            {
                _notificationService.ShowBookingConfirmed(SelectedCompany.CompanyName, SelectedCompany.JobTitle, SelectedSlot.StartTime, SelectedSlot.EndTime);
            }
            catch { }

            MatchedCompanies.Remove(SelectedCompany);
            IsBookingVisible = false;
        }

        private void Join(object obj)
        {
            try
            {
                var page = new InterviewCandidatePage((InterviewSession)obj);
                var window = new Window();
                page.Tag = window;

                page.OnClosed = () => _ = LoadInterviewSessionsAsync();

                window.Content = page;
                window.Activate();
            }
            catch { }
        }

        private async void Cancel(object obj)
        {
            if (obj is InterviewSession session)
            {
                try
                {
                    var connectedInterviewSession = await _interviewSessionRepo.GetInterviewSessionByIdAsync(session.Id);
                    if (connectedInterviewSession != null)
                    {
                        _interviewSessionRepo.Delete(connectedInterviewSession);
                    }



                    await LoadInterviewSessionsAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Cancellation failed: {ex.Message}");
                }
            }
        }
    }
}