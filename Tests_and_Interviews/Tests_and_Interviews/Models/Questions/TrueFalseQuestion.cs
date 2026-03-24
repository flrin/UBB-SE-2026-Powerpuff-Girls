using System.ComponentModel.DataAnnotations.Schema;
using Tests_and_Interviews.Models.Core;

namespace Tests_and_Interviews.Models.Questions
{
    
    public class TrueFalseQuestion : Question
    {
        
        [NotMapped]
        public bool CorrectAnswer
        {
            get => bool.TryParse(QuestionAnswer, out var result) && result;
            set => QuestionAnswer = value.ToString().ToLower();
        }

        [NotMapped]
        public bool? UserAnswerBool { get; set; }
    }
}