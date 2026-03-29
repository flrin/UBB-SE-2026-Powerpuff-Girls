using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tests_and_Interviews.Models.Core
{
    [Table("LeaderboardEntries")]
    public class LeaderboardEntry
    {
        [Key]
        [Column("leaderboard_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("test_id")]
        public int TestId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("normalized_score")]
        public decimal NormalizedScore { get; set; }

        [Column("rank_position")]
        public int RankPosition { get; set; }

        [Column("tie_break_priority")]
        public int TieBreakPriority { get; set; }

        [Column("last_recalculation_at")]
        public DateTime LastRecalculationAt { get; set; }

        public Test? Test { get; set; }
        public User? User { get; set; }
    }
}