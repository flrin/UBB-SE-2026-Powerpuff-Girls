using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tests_and_Interviews.Models.Core
{
    [Table("interview_sessions")]
    public class InterviewSession
    {
        [Key]
        [Column("session_id")]
        public int Id { get; set; }

        [NotMapped]
        public int SessionId
        {
            get => Id;
            set => Id = value;
        }

        [Column("position_id")]
        public int PositionId { get; set; }

        [Column("external_user_id")]
        public int ExternalUserId { get; set; }

        [Column("interviewer_id")]
        public int InterviewerId { get; set; }

        [Column("date_start")]
        public DateTime DateStart { get; set; }

        [Column("video")]
        [MaxLength(200)]
        public string Video { get; set; } = string.Empty;

        [Column("status")]
        [MaxLength(200)]
        public string Status { get; set; } = string.Empty;

        [Column("score")]
        public decimal Score { get; set; }
    }
}