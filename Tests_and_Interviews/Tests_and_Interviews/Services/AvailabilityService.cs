using System;
using System.Collections.Generic;
using System.Linq;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Repositories;

namespace Tests_and_Interviews.Services
{
    public class AvailabilityService
    {
        private readonly SlotRepository _slotRepo;

        public AvailabilityService()
        {
            _slotRepo = new SlotRepository();
        }

        public void CreateSlot(DateTime startTime, int duration, int recruiterId)
        {
            var slot = new Slot
            {
                Id = new Random().Next(1000, 9999),
                RecruiterId = recruiterId,
                StartTime = startTime,
                EndTime = startTime.AddMinutes(duration),
                Duration = duration,
                Status = SlotStatus.Free,
                InterviewType = "Technical"
            };

            _slotRepo.Add(slot);
        }

        public List<Slot> GetRecruiterCalendar(int recruiterId, DateTime month)
        {
            return _slotRepo
                .GetSlots(recruiterId, month)
                .Where(s => s.StartTime.Month == month.Month &&
                            s.StartTime.Year == month.Year)
                .ToList();
        }

        public Slot? UpdateSlot(int slotId, Slot updatedSlot)
        {
            var existing = _slotRepo.GetById(slotId);

            if (existing == null)
                return null;

            existing.StartTime = updatedSlot.StartTime;
            existing.EndTime = updatedSlot.EndTime;
            existing.Duration = updatedSlot.Duration;
            existing.Status = updatedSlot.Status;
            existing.InterviewType = updatedSlot.InterviewType;

            _slotRepo.Update(existing);

            return existing;
        }

        public Slot? DeleteSlot(int slotId)
        {
            var slot = _slotRepo.GetById(slotId);

            if (slot != null)
            {
                _slotRepo.Delete(slotId);
            }

            return slot;
        }
    }
}
