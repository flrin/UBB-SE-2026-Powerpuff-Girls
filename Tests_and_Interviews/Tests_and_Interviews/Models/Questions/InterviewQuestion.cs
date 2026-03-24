using System.ComponentModel.DataAnnotations.Schema;
using Tests_and_Interviews.Models.Core;

namespace Tests_and_Interviews.Models.Questions
{
     public class InterviewQuestion : Question
    {
       
        [NotMapped]
        public string Notes { get; set; } = string.Empty;

        [NotMapped]
        public string UserResponse { get; set; } = string.Empty;
    }
}