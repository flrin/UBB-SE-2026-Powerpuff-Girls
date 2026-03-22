using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tests_and_Interviews.Models.Enums;

namespace Tests_and_Interviews.Models.Core
{
    [Table("questions")]
    public class Question
    {
        [Key]
        [Column("question_id")]
        public int Id { get; set; }

        [Column("position_id")]
        public int? PositionId { get; set; }

        [Column("test_id")]
        public int? TestId { get; set; }

        [Column("question_text")]
        [MaxLength(200)]
        public string QuestionText { get; set; } = string.Empty;

        [Column("question_type")]
        [MaxLength(200)]
        public string QuestionTypeString { get; set; } = string.Empty;

        [Column("question_score")]
        public float QuestionScore { get; set; }

        [Column("question_answer")]
        [MaxLength(200)]
        public string? QuestionAnswer { get; set; }

        [Column("options_json")]
        [MaxLength(1000)]
        public string? OptionsJson { get; set; }

        [NotMapped]
        public QuestionType Type => QuestionTypeString switch
        {
            "SINGLE_CHOICE" => QuestionType.SINGLE_CHOICE,
            "MULTIPLE_CHOICE" => QuestionType.MULTIPLE_CHOICE,
            "TEXT" => QuestionType.TEXT,
            "TRUE_FALSE" => QuestionType.TRUE_FALSE,
            "INTERVIEW" => QuestionType.INTERVIEW,
            _ => QuestionType.TEXT
        };

        public Test? Test { get; set; }
        public List<Answer> Answers { get; set; } = new();
    }
}