using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests_and_Interviews.Models;


namespace Tests_and_Interviews.Repositories
{
        public class RecruiterRepository
        {
            private static List<Recruiter> _recruiters = new List<Recruiter>();

            public Recruiter? FindById(int id)
            {
                return _recruiters.FirstOrDefault(r => r.CompanyId == id);
            }

            public List<Slot> GetCalendar(int recruiterId)
            {
                var recruiter = FindById(recruiterId);
                return recruiter?.Slots ?? new List<Slot>();
            }

            public void Save(Recruiter recruiter)
            {
                var existing = FindById(recruiter.CompanyId);

                if (existing == null)
                    _recruiters.Add(recruiter);
                else
                    _recruiters[_recruiters.IndexOf(existing)] = recruiter;
            }
        }
    
}
