using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tests_and_Interviews.Models.Enums;

namespace Tests_and_Interviews.Models.Core
{
    [Table("test_attempts")]
    public class TestAttempt
    {
        [Key]
        [Column("userTest_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("test_id")]
        public int TestId { get; set; }

        
        [Column("external_user_id")]
        public int? ExternalUserId { get; set; }

        [NotMapped]
        public int? UserId
        {
            get => ExternalUserId;
            set => ExternalUserId = value;
        }

        [Column("score")]
        public decimal? Score { get; set; }

        [Column("status")]
        [MaxLength(200)]
        public string Status { get; set; } = TestStatus.NOT_STARTED.ToString();

        [Column("started_at")]
        public DateTime? StartedAt { get; set; }

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [Column("answers_file_path")]
        [MaxLength(200)]
        public string AnswersFilePath { get; set; } = string.Empty;

        [Column("is_validated")]
        public bool IsValidated { get; set; } = false;

        [Column("percentage_score")]
        public decimal? PercentageScore { get; set; }

        [Column("rejection_reason")]
        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        [Column("rejected_at")]
        public DateTime? RejectedAt { get; set; }

        public List<Answer> Answers { get; set; } = new();
        public Test? Test { get; set; }
        public User? User { get; set; }

         public void Start()
        {
            Status = TestStatus.RECORDING.ToString();
            StartedAt = DateTime.UtcNow;
        }

        public void Submit()
        {
            Status = TestStatus.COMPLETED.ToString();
            CompletedAt = DateTime.UtcNow;
        }

        public void Expire()
        {
            Status = TestStatus.SUBMITTED.ToString();
            CompletedAt = DateTime.UtcNow;
        }
        public float CalculateScore()
        {
            if (Answers.Count == 0 || Test?.Questions.Count is null or 0)
                return 0f;

            float maxPossible = 0f;
            foreach (var q in Test.Questions)
                maxPossible += q.QuestionScore;

            float scoreValue = (float)(Score ?? 0m);
            return maxPossible == 0f ? 0f : scoreValue / maxPossible * 100f;
        }
    }
}