using System;
namespace QuestionProConsole.Models
{
	public class Results
    {
        public long responseID { get; set; }
        public long surveyID { get; set; }
        public string? surveyName { get; set; }
        public string? ipAddress { get; set; }
        public string? timestamp { get; set; }
        public string? dataQuality { get; set; }
        public double? dataQualityScore { get; set; }
        public Location? location { get; set; }
        public bool? duplicate { get; set; }
        public int? timeTaken { get; set; }
        public string? responseStatus { get; set; }
        public string? completionUrl { get; set; }
        public string? externalReference { get; set; }
        public CustomVariables? customVariables { get; set; }
        public string? language { get; set; }
        public string? currentInset { get; set; }
        public string? operatingSystem { get; set; }
        public string? osDeviceType { get; set; }
        public string? browser { get; set; }
        public List<ResponseSet>? responseSet { get; set; }
        public long? utctimestamp { get; set; }
    }
}

