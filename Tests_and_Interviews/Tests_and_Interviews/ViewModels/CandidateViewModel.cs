using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Tests_and_Interviews.Helpers;
using Tests_and_Interviews.Services;
using Tests_and_Interviews.Domain;

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
            AvailableSlots = _bookingService.getAvailableSlots(1, DateTime.Today);
        }
    }
}