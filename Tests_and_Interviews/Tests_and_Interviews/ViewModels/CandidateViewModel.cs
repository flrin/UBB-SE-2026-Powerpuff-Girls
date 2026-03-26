using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Tests_and_Interviews.Helpers;
using Tests_and_Interviews.Services;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews;
using Tests_and_Interviews.Models.Enums;
using Tests_and_Interviews.Models;

namespace Tests_and_Interviews.ViewModels
{
    public class CandidateViewModel : INotifyPropertyChanged
    {
        private readonly BookingService _bookingService;
        private readonly NotificationService _notificationService;
        private List<Slot> _availableSlots;
        private List<Slot> _availableDays;
        private ObservableCollection<Company> _matchedCompanies;
        private Company _selectedCompany;
        private Slot _selectedSlot;
        private DateTime _selectedDay;
        private int _dayStartIndex = 0;
        private bool _isBookingVisible;

        public ICommand LoadSlotsCommand { get; }
        public ICommand ScheduleCommand { get; }
        public ICommand SelectDayCommand { get; }
        public ICommand SelectSlotCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand NextDaysCommand { get; }
        public ICommand PrevDaysCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public CandidateViewModel()
        {
            _bookingService = new BookingService();
            _notificationService = new NotificationService();
            LoadSlotsCommand = new RelayCommand(_ => LoadSlots());
            ScheduleCommand = new RelayCommand((obj) => Schedule((Company)obj));

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

            SelectSlotCommand = new RelayCommand((obj) =>
            {
                var selected = (Slot)obj;
                if (AvailableSlots == null) return;

                foreach (var s in AvailableSlots)
                    s.IsSlotSelected = false;

                selected.IsSlotSelected = true;
                SelectedSlot = selected;
                OnPropertyChanged(nameof(AvailableSlots));
            });

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

            // initial sample data
            MatchedCompanies = new ObservableCollection<Company>
            {
                new Company { CompanyName = "Google", JobTitle = "Frontend Dev", RecruiterId = 1 },
                new Company { CompanyName = "Amazon", JobTitle = "Backend Dev", RecruiterId = 2 }
            };
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

        public IEnumerable<Slot> VisibleDays => (AvailableDays ?? new List<Slot>())
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
            set { _selectedSlot = value; OnPropertyChanged(); }
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
            // Populate matched companies or load from a service if available
            MatchedCompanies = new ObservableCollection<Company>
            {
                new Company { CompanyName = "Google", JobTitle = "Frontend Dev", RecruiterId = 1 },
                new Company { CompanyName = "Amazon", JobTitle = "Backend Dev", RecruiterId = 2 }
            };
        }

        private void Schedule(Company company)
        {
            IsBookingVisible = true;
            SelectedCompany = company;

            var slots = _bookingService.GetAllAvailableSlots(company.RecruiterId);

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
                .GetAllAvailableSlots(SelectedCompany.RecruiterId)
                .Where(s => s.StartTime.Date == SelectedDay.Date)
                .ToList();
        }

        private void ConfirmBooking(object obj)
        {
            if (SelectedSlot == null)
                return;

            _bookingService.ConfirmBooking(1, SelectedSlot.Id);
            try
            {
                _notificationService.ShowBookingConfirmed(SelectedCompany.CompanyName, SelectedCompany.JobTitle, SelectedSlot.StartTime, SelectedSlot.EndTime);
            }
            catch { }
            MatchedCompanies.Remove(SelectedCompany);
            IsBookingVisible = false;
        }
    }
}