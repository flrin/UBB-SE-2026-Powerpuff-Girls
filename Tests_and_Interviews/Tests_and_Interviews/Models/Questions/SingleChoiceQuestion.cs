using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Tests_and_Interviews.Models.Core;

namespace Tests_and_Interviews.Models.Questions
{

    public class SingleChoiceQuestion : Question
    {
        [NotMapped]
        public new List<string> Options { get; set; } = [];

        [NotMapped]
        public int CorrectAnswerIndex { get; set; }

        [NotMapped]
        public int? SelectedIndex { get; set; }
    }
}