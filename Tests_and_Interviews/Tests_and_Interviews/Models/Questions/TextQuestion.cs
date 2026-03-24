using System.ComponentModel.DataAnnotations.Schema;
using Tests_and_Interviews.Models.Core;

namespace Tests_and_Interviews.Models.Questions
{
    
    public class TextQuestion : Question
    {
    
        [NotMapped]
        public string CorrectAnswerText
        {
            get => QuestionAnswer ?? string.Empty;
            set => QuestionAnswer = value;
        }

        [NotMapped]
        public string UserAnswerText { get; set; } = string.Empty;
    }
}