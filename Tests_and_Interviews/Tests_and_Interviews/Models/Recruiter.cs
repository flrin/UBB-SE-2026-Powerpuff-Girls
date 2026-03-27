using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests_and_Interviews.Models
{
    public class Recruiter
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public List<Candidate> AssignedCandidates { get; set; } = [];
        public List<Slot> Slots { get; set; } = [];
        public List<Slot> ViewMonthlyCalendar(DateTime month)
        {
            return Slots
                .Where(s => s.StartTime.Month == month.Month && s.StartTime.Year == month.Year)
                .ToList();
        }

        public void CreateSlot(Slot slot)
        {
            Slots.Add(slot);
        }

        public void EditSlot(Slot updatedSlot)
        {
            var existing = Slots.FirstOrDefault(s => s.Id == updatedSlot.Id);
            if (existing != null)
            {
                existing.StartTime = updatedSlot.StartTime;
                existing.EndTime = updatedSlot.EndTime;
                existing.Duration = updatedSlot.Duration;
                existing.Status = updatedSlot.Status;
                existing.InterviewType = updatedSlot.InterviewType;
            }
        }

        public Slot? DeleteSlot(int slotId)
        {
            var slot = Slots.FirstOrDefault(s => s.Id == slotId);
            if (slot != null)
            {
                Slots.Remove(slot);
            }
            return slot;
        }
    }
}

