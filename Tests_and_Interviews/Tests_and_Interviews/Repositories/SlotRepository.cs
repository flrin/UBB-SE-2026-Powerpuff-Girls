using System;
using System.Collections.Generic;
using System.Linq;
using Tests_and_Interviews.Models;

namespace Tests_and_Interviews.Repositories
{
    public class SlotRepository
    {
        private static readonly List<Slot> _slots = new List<Slot>();

        public List<Slot> GetSlots(int recruiterId, DateTime date)
        {
            return _slots
                .Where(s => s.RecruiterId == recruiterId &&
                            s.StartTime.Date == date.Date)
                .OrderBy(s => s.StartTime)
                .ToList();
        }

        public Slot? GetById(int id)
        {
            return _slots.FirstOrDefault(s => s.Id == id);
        }

        public void Add(Slot slot)
        {
            if (slot == null)
                throw new ArgumentNullException(nameof(slot));

            bool overlap = _slots.Any(s =>
                s.RecruiterId == slot.RecruiterId &&
                s.StartTime.Date == slot.StartTime.Date &&
                slot.StartTime < s.EndTime &&
                slot.EndTime > s.StartTime
            );

            if (overlap)
                throw new Exception("Slot overlaps!");

            _slots.Add(slot);
        }

        public void Update(Slot slot)
        {
            if (slot == null)
                throw new ArgumentNullException(nameof(slot));

            var index = _slots.FindIndex(s => s.Id == slot.Id);

            if (index == -1)
                throw new Exception("Slot not found");

            _slots[index] = slot;
        }

        public void Delete(int id)
        {
            var slot = _slots.FirstOrDefault(s => s.Id == id);

            if (slot != null)
                _slots.Remove(slot);
        }
    }
}
