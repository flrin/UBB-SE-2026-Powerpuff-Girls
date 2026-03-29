using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tests_and_Interviews.Models.Enums;
using Tests_and_Interviews.Repositories;

namespace Tests_and_Interviews.Models
{
    public class Slot : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int RecruiterId { get; set; }
        public int CandidateId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Duration { get; set; }
        public SlotStatus Status { get; set; }
        public string InterviewType { get; set; } = string.Empty;
        public string FormattedTime => StartTime.ToString("HH:mm");
        public string TimeRange => $"{StartTime:HH:mm} - {EndTime:HH:mm}";
        public string DayFormatted => StartTime.ToString("dd MMM");

        public int RowSpan => Duration > 0 ? Duration / 30 : 1;
        public bool IsOccupied => Status == SlotStatus.Occupied;
        public bool IsAvailable => Status == SlotStatus.Free;

        private bool _isDaySelected;
        public bool IsDaySelected
        {
            get => _isDaySelected;
            set
            {
                _isDaySelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BackgroundColor));
                OnPropertyChanged(nameof(ForegroundColor));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public string BackgroundColor => IsDaySelected ? "#6367FF" : "#C9BEFF";
        public string ForegroundColor => IsDaySelected ? "White" : "Black";
        public string SlotColor => IsSlotSelected ? "#8494FF" : "#FFDBFD";
        public bool IsHidden { get; set; }




        public void Lock(int candidateId)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("Slot is not available");

            Status = SlotStatus.Occupied;
            CandidateId = candidateId;

            var repo = new SlotRepository();
            repo.Update(this);
        }

        private bool _isSlotSelected;
        public bool IsSlotSelected
        {
            get => _isSlotSelected;
            set
            {
                _isSlotSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SlotColor));
            }
        }
        public void Release()
        {
            Status = SlotStatus.Free;
            CandidateId = 0;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}