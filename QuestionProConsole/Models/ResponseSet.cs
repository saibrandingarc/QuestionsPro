using System;
using System.Collections.Generic;

namespace QuestionProConsole.Models
{
	public class ResponseSet
	{
        public long? questionID { get; set; }
        public string? questionDescription { get; set; }
        public string? questionCode { get; set; }
        public string? questionText { get; set; }
        public string? questionType { get; set; }
        public string? imageUrl { get; set; }
        public List<AnswerValue>? answerValues { get; set; }
    }
}

