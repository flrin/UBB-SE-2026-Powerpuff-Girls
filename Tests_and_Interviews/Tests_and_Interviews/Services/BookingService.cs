using System;
using System.Collections.Generic;
using System.Linq;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Repositories;

namespace Tests_and_Interviews.Services
{
    public class BookingService
    {
        private readonly SlotRepository _slotRepo;
        private readonly InterviewSessionRepository _interviewRepo;

        public BookingService()
        {
            _slotRepo = new SlotRepository();
            _interviewRepo = new InterviewSessionRepository();
        }

        public List<Slot> GetAvailableSlots(int recruiterId, DateTime date)
        {
            return _slotRepo
                .GetSlots(recruiterId, date)
                .Where(s => s.Status == SlotStatus.Free)
                .OrderBy(s => s.StartTime)
                .ToList();
        }

        public List<Slot> GetAvailableSlotsByRecruiterId(int recruiterId)
        {
            return _slotRepo
                .GetAllSlots(recruiterId)
                .Where(s => s.Status == SlotStatus.Free)
                .OrderBy(s => s.StartTime)
                .ToList();
        }

        public void ConfirmBooking(int candidateId, Slot slot)
        {
            if (slot == null)
                throw new Exception("Slot not found");

            if (slot.Status != SlotStatus.Free)
                throw new Exception("This salot is no longer available");

            slot.Status = SlotStatus.Occupied;
            slot.CandidateId = candidateId;
            slot.InterviewType = "";

            _slotRepo.Update(slot);

            InterviewSession newInterviewSession = new InterviewSession
            {
                SessionId = slot.Id,
                PositionId = 0,
                ExternalUserId = candidateId,
                InterviewerId = slot.RecruiterId,
                DateStart = slot.StartTime.ToUniversalTime(),
                Video = "",
                Status = "Scheduled",
                Score = 0
            };

            _interviewRepo.Add(newInterviewSession);
        }
    }
}