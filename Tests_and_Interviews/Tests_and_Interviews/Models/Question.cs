using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests_and_Interviews.Models
{
    public class Question
    {
        public int Id { get; set; }
        public int PositionId { get; set; }
        public int TestId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public float QuestionScore { get; set; }
        public string QuestionAnswer { get; set; }
    }
}
