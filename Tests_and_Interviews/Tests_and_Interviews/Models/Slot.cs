using System;

namespace Tests_and_Interviews.Domain
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
		public string InterviewType { get; set; }

		public bool IsAvailable()
		{
			return Status == SlotStatus.Free;
		}

		public void Lock(int candidateId)
		{
			if (!IsAvailable())
				throw new Exception("Slot is not available");

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