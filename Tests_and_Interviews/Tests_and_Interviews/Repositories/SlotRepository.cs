using System;
using System.Collections.Generic;
using System.Linq;
using Tests_and_Interviews.Domain;

namespace Tests_and_Interviews.Repositories
{
	public class SlotRepository
	{
		private static List<Slot> _slots = new List<Slot>
		{
			new Slot
			{
				Id = 1,
				RecruiterId = 1,
				StartTime = DateTime.Today.AddHours(10),
				EndTime = DateTime.Today.AddHours(10.5),
				Duration = 30,
				Status = SlotStatus.Free,
			},
			new Slot
			{
				Id = 2,
				RecruiterId = 1,
				StartTime = DateTime.Today.AddHours(11),
				EndTime = DateTime.Today.AddHours(11.5),
				Duration = 30,
				Status = SlotStatus.Free,
			}
		};

		public List<Slot> GetSlots(int recruiterId, DateTime date)
		{
			return _slots
				.Where(s => s.RecruiterId == recruiterId &&
							s.StartTime.Date == date.Date)
				.ToList();
		}

		public Slot GetById(int id)
		{
			return _slots.FirstOrDefault(s => s.Id == id);
		}

		public void Update(Slot slot)
		{
			var index = _slots.FindIndex(s => s.Id == slot.Id);
			if (index != -1)
				_slots[index] = slot;
		}
	}
}