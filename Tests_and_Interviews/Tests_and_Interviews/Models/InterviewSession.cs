using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests_and_Interviews.Models
{
    public class InterviewSession
    {
        public int Id { get; set; }
        public int PositionId{ get; set; }
        public int ExternalUserId { get; set; }
        public int InterviewerId { get; set; }
        public DateTime DateStart { get; set; }
        public string Video { get; set; }
        public string Status { get; set; }
        public float Score { get; set; }
    }
}
