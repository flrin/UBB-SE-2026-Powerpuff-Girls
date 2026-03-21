using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tests_and_Interviews.Models.Core
{
    [Table("answers")]
    public class Answer
    {
        [Key]
        [Column("answer_id")]
        public int Id { get; set; }

        [Column("question_id")]
        public int QuestionId { get; set; }

        [Column("attempt_id")]
        public int AttemptId { get; set; }

        [Column("value")]
        public string Value { get; set; } = string.Empty;

        public Question? Question { get; set; }
        public TestAttempt? TestAttempt { get; set; }
    }
}