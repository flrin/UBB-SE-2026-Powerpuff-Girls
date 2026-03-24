using System;
using System.Collections.Generic;
using System.Linq;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Services;

namespace Tests_and_Interviews.Repositories
{
    public class SlotRepository
    {
        
        private List<Slot> GetAll()
        {
            return SlotJsonService.LoadSlots();
        }

     
        private void SaveAll(List<Slot> slots)
        {
            SlotJsonService.SaveSlots(slots);
        }

        public List<Slot> GetSlots(int recruiterId, DateTime date)
        {
            return GetAll()
                .Where(s => s.RecruiterId == recruiterId &&
                            s.StartTime.Date == date.Date)
                .OrderBy(s => s.StartTime)
                .ToList();
        }

      
        public List<Slot> GetAllSlots(int recruiterId)
        {
            return GetAll()
                .Where(s => s.RecruiterId == recruiterId)
                .OrderBy(s => s.StartTime)
                .ToList();
        }

     
        public Slot? GetById(int id)
        {
            return GetAll().FirstOrDefault(s => s.Id == id);
        }

       
        public void Add(Slot slot)
        {
            var slots = GetAll();

            bool overlap = slots.Any(s =>
                s.RecruiterId == slot.RecruiterId &&
                s.StartTime.Date == slot.StartTime.Date &&
                slot.StartTime < s.EndTime &&
                slot.EndTime > s.StartTime
            );

            if (overlap)
                throw new Exception("Slot overlaps!");

       
            slot.Id = slots.Any() ? slots.Max(s => s.Id) + 1 : 1;

            slots.Add(slot);
            SaveAll(slots);
        }

       
        public void Update(Slot slot)
        {
            var slots = GetAll();

            var index = slots.FindIndex(s => s.Id == slot.Id);
            if (index == -1)
                throw new Exception("Slot not found");

            slots[index] = slot;
            SaveAll(slots);
        }

     
        public void Delete(int id)
        {
            var slots = GetAll();

            var slot = slots.FirstOrDefault(s => s.Id == id);
            if (slot != null)
            {
                slots.Remove(slot);
                SaveAll(slots);
            }
        }
    }
}