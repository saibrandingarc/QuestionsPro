using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionProConsole.Models
{
    [Table("QuestionsLookup")]
    public class QuestionsLookup
	{
        [Key]
        public int Id { get; set; }

        public int? QuestionId { get; set; }

        [MaxLength(50)]
        public string QuestionCode { get; set; }

        [MaxLength(50)]
        public string TargetColumn { get; set; }

        public int? AnswerId { get; set; }
    }
}

