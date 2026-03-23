using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Tests_and_Interviews.Helpers;
using Tests_and_Interviews.Services;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Repositories;

namespace Tests_and_Interviews.ViewModels
{
    public class CandidateViewModel : INotifyPropertyChanged
    {
        private readonly BookingService _bookingService;
        private List<Slot> _availableSlots;
        private ObservableCollection<Company> _matchedCompanies;
        private Company _selectedCompany;
        private bool _isBookingVisible;
        private List<Slot> _availableDays;
        private DateTime _selectedDay;
        private Slot _selectedSlot;
        private int _dayStartIndex = 0;

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

        public bool IsBookingVisible
        {
            get => _isBookingVisible;
            set { _isBookingVisible = value; OnPropertyChanged(); }
        }

        public List<Slot> AvailableSlots
        {
            get => _availableSlots;
            set { _availableSlots = value; OnPropertyChanged(); }
        }

        public List<Slot> AvailableDays
        {
            get => _availableDays;
            set
            {
                _availableDays = value;
                _dayStartIndex = 0;
                OnPropertyChanged();
                OnPropertyChanged(nameof(VisibleDays));
            }
        }

        public List<Slot> VisibleDays =>
            AvailableDays?.Skip(_dayStartIndex).Take(3).ToList();

        public DateTime SelectedDay
        {
            get => _selectedDay;
            set
            {
                _selectedDay = value;
                OnPropertyChanged();
                LoadSlotsForSelectedDay();
            }
        }

        public Slot SelectedSlot
        {
            get => _selectedSlot;
            set
            {
                _selectedSlot = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Company> MatchedCompanies
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

            var repo = new SlotRepository();

            repo.Add(new Slot { Id = 1, RecruiterId = 1, StartTime = DateTime.Today.AddHours(10), EndTime = DateTime.Today.AddHours(10).AddMinutes(45), Duration = 45, Status = SlotStatus.Free });
            repo.Add(new Slot { Id = 2, RecruiterId = 1, StartTime = DateTime.Today.AddHours(11), EndTime = DateTime.Today.AddHours(11).AddMinutes(45), Duration = 45, Status = SlotStatus.Free });
            repo.Add(new Slot { Id = 3, RecruiterId = 1, StartTime = DateTime.Today.AddHours(12), EndTime = DateTime.Today.AddHours(12).AddMinutes(45), Duration = 45, Status = SlotStatus.Free });
       
            repo.Add(new Slot { Id = 4, RecruiterId = 1, StartTime = DateTime.Today.AddDays(1).AddHours(10), EndTime = DateTime.Today.AddDays(1).AddHours(10).AddMinutes(45), Duration = 45, Status = SlotStatus.Free });
            repo.Add(new Slot { Id = 5, RecruiterId = 1, StartTime = DateTime.Today.AddDays(1).AddHours(11), EndTime = DateTime.Today.AddDays(1).AddHours(11).AddMinutes(45), Duration = 45, Status = SlotStatus.Free });

            repo.Add(new Slot { Id = 6, RecruiterId = 2, StartTime = DateTime.Today.AddHours(13), EndTime = DateTime.Today.AddHours(13).AddMinutes(45), Duration = 45, Status = SlotStatus.Free });
            repo.Add(new Slot { Id = 7, RecruiterId = 2, StartTime = DateTime.Today.AddHours(14), EndTime = DateTime.Today.AddHours(14).AddMinutes(45), Duration = 45, Status = SlotStatus.Free });
            
            repo.Add(new Slot { Id = 8, RecruiterId = 2, StartTime = DateTime.Today.AddDays(1).AddHours(15), EndTime = DateTime.Today.AddDays(1).AddHours(15).AddMinutes(45), Duration = 45, Status = SlotStatus.Free });
            repo.Add(new Slot { Id = 9, RecruiterId = 2, StartTime = DateTime.Today.AddDays(1).AddHours(16), EndTime = DateTime.Today.AddDays(1).AddHours(16).AddMinutes(45), Duration = 45, Status = SlotStatus.Free });

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

            _bookingService.confirmBooking(1, SelectedSlot.Id);
            MatchedCompanies.Remove(SelectedCompany);
            IsBookingVisible = false;
        }
    }
}