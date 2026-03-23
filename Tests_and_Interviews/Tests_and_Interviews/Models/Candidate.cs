using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests_and_Interviews.Models
{
        public class Candidate
        {
            public int Id { get; set; }
            public int AssignedRecruiterId { get; set; }
            public string ApplicationStatus { get; set; } = string.Empty;
            public string MatchedCompany { get; set; } = string.Empty;
            public List<Slot> AvailableSlots { get; set; } = new List<Slot>();
            
        }
    
}
