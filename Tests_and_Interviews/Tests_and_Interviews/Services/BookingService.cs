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
        private readonly AppDbContext _dbContext;

        public BookingService()
        {
            _slotRepo = new SlotRepository();
            _dbContext = new AppDbContext();
        }


        public List<Slot> GetAvailableSlots(int recruiterId, DateTime date)
        {
            return _slotRepo
                .GetSlots(recruiterId, date)
                .Where(s => s.Status == SlotStatus.Free)
                .OrderBy(s => s.StartTime)
                .ToList();
        }
        public List<Slot> GetAllAvailableSlots(int recruiterId)
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
                throw new Exception("This slot is no longer available");

            slot.Status = SlotStatus.Occupied;
            slot.CandidateId = candidateId;

            _slotRepo.Update(slot);

            InterviewSession newInterviewSession = new InterviewSession
            {
                SessionId = slot.Id,
                PositionId = 0, // This should be set based on the actual position being applied for
                ExternalUserId = candidateId,
                InterviewerId = slot.RecruiterId,
                DateStart = slot.StartTime.ToUniversalTime(),
                Video = "",
                Status = "Scheduled",
                Score = 0
            };

            _dbContext.InterviewSessions.Add(newInterviewSession);
            _dbContext.SaveChanges();
        }

        public void confirmBooking(int candidateId, Slot slot)
        {
            ConfirmBooking(candidateId, slot);
        }
    }
}