using System;
using System.Collections.Generic;
using System.Linq;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Repositories;

namespace Tests_and_Interviews.Services
{
    public class BookingService
    {
        private readonly SlotRepository _slotRepo;

        public BookingService()
        {
            _slotRepo = new SlotRepository();
        }

        public List<Slot> GetAvailableSlots(int recruiterId, DateTime date)
        {
            return _slotRepo
                .GetSlots(recruiterId, date)
                .Where(s => s.IsAvailable)
                .ToList();
        }


        public List<Slot> getAvailableSlots(int recruiterId, DateTime date)
        {
            return GetAvailableSlots(recruiterId, date);
        }

        public void ConfirmBooking(int candidateId, int slotId)
        {
            var slot = _slotRepo.GetById(slotId);

            if (slot == null)
                throw new Exception("Slot not found");

            if (!slot.IsAvailable)
                throw new Exception("This slot is no longer available");

            slot.Lock(candidateId);
            _slotRepo.Update(slot);
        }
        public void confirmBooking(int candidateId, int slotId)
        {
            ConfirmBooking(candidateId, slotId);
        }
    }
}