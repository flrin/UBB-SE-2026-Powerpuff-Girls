using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Tests_and_Interviews.Models.Core;

namespace Tests_and_Interviews.Models.Questions
{
    
    public class MultipleChoiceQuestion : Question
    {
        [NotMapped]
        public new List<string> Options { get; set; } = new();

       
        [NotMapped]
        public new List<int> CorrectAnswerIndexes { get; set; } = new();

        [NotMapped]
        public List<int> SelectedIndexes { get; set; } = new();
    }
}