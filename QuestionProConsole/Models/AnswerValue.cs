using System;
using Newtonsoft.Json.Linq;

namespace QuestionProConsole.Models
{
	public class AnswerValue
	{
        public long answerID { get; set; }
        public string answerText { get; set; }
        public ValueData value { get; set; }
    }
}

