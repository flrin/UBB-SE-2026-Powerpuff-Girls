using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Tests_and_Interviews.Helpers;
using Tests_and_Interviews.Services;
using Tests_and_Interviews.Models;

namespace Tests_and_Interviews.ViewModels
{
    public class CandidateViewModel : INotifyPropertyChanged
    {
        private readonly BookingService _bookingService;
        private List<Slot> _availableSlots;
        public ICommand LoadSlotsCommand { get; }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public CandidateViewModel()
        {
            _bookingService = new BookingService();
            LoadSlotsCommand = new RelayCommand(LoadSlots);

            LoadSlots();
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

        private void LoadSlots()
        {
            get => _matchedCompanies;
            set { _matchedCompanies = value; OnPropertyChanged(); }
        }

        public Company SelectedCompany
        {
            get => _selectedCompany;
            set { _selectedCompany = value; OnPropertyChanged(); }
        }

        public CandidateViewModel()
        {
            _bookingService = new BookingService();
            ScheduleCommand = new RelayCommand((obj) => Schedule((Company)obj));
            SelectDayCommand = new RelayCommand((obj) =>
            {
                var selected = (Slot)obj;

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

                foreach (var s in AvailableSlots)
                    s.IsDaySelected = false;

                selected.IsDaySelected = true;
                SelectedSlot = selected;
                OnPropertyChanged(nameof(AvailableSlots));
            });

            SelectDayCommand = new RelayCommand((obj) =>
            {
                var selected = (Slot)obj;

                foreach (var d in AvailableDays)
                    d.IsDaySelected = false;
                selected.IsDaySelected = true;
                SelectedDay = selected.StartTime.Date;

                foreach (var s in AvailableSlots ?? new List<Slot>())
                    s.IsSlotSelected = false;
                SelectedSlot = null;

                OnPropertyChanged(nameof(AvailableDays));
            });

            SelectSlotCommand = new RelayCommand((obj) =>
            {
                var selected = (Slot)obj;
                foreach (var s in AvailableSlots)
                    s.IsSlotSelected = false;

                selected.IsSlotSelected = true;
                SelectedSlot = selected;
                OnPropertyChanged(nameof(AvailableSlots));
            });
            ConfirmCommand = new RelayCommand(ConfirmBooking);

            NextDaysCommand = new RelayCommand(_ =>
            {
                if (_dayStartIndex + 3 < AvailableDays.Count)
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
            MatchedCompanies.Remove(SelectedCompany);
            IsBookingVisible = false;
        }
    }
}