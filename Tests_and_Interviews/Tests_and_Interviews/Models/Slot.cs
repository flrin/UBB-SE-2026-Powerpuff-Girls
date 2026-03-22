using System;

namespace Tests_and_Interviews.Models
{
    public class Slot
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
        public int RowSpan => Duration > 0 ? Duration / 30 : 1;
        public bool IsOccupied => Status == SlotStatus.Occupied;
        public bool IsAvailable => Status == SlotStatus.Free;

        public void Lock(int candidateId)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("Slot is not available");

            Status = SlotStatus.Occupied;
            CandidateId = candidateId;
        }

        public void Release()
        {
            Status = SlotStatus.Free;
            CandidateId = 0;
        }
    }
}