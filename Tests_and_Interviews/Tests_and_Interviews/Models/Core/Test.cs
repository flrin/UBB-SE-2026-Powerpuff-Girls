using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tests_and_Interviews.Models.Core
{
    [Table("tests")]
    public class Test
    {
        [Key]
        [Column("test_id")]
        public int Id { get; set; }

        [Column("title")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column("category")]
        [MaxLength(200)]
        public string Category { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public List<Question> Questions { get; set; } = new();
        public List<TestAttempt> Attempts { get; set; } = new();
    }
}