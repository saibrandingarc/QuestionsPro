using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionProConsole.Models
{
    [Table("QuestionsPro")]
    public class QuestionsPro
    {
        [Key]
        public int Id { get; set; }

        [Column("Response ID")]
        public string? ResponseID { get; set; }

        [Column("Response Status")]
        public string? ResponseStatus { get; set; }

        [Column("IP Address")]
        public string? IPAddress { get; set; }

        [Column("Timestamp (mm dd yyyy)")]
        public DateTime? Timestamp { get; set; }

        public string? Duplicate { get; set; }

        [Column("Time Taken to Complete (Seconds)")]
        public int? TimeTakenToComplete { get; set; }

        [Column("Seq  Number")]
        public int? SeqNumber { get; set; }

        [Column("External Reference")]
        public string? ExternalReference { get; set; }

        [Column("Custom Variable 1")]
        public string? CustomVariable1 { get; set; }

        [Column("Custom Variable 2")]
        public string? CustomVariable2 { get; set; }

        [Column("Custom Variable 3")]
        public string? CustomVariable3 { get; set; }

        [Column("Custom Variable 4")]
        public string? CustomVariable4 { get; set; }

        [Column("Custom Variable 5")]
        public string? CustomVariable5 { get; set; }

        [Column("Respondent Email")]
        public string? RespondentEmail { get; set; }

        [Column("Email List")]
        public string? EmailList { get; set; }

        [Column("Country Code")]
        public string? CountryCode { get; set; }

        public string? Region { get; set; }

        [Column("Survey Introduction and Instructions")]
        public string? SurveyIntroductionAndInstructions { get; set; }

        [Column("First Name")]
        public string? FirstName { get; set; }

        [Column("Last Name")]
        public string? LastName { get; set; }

        [Column("Email Address")]
        public string? EmailAddress { get; set; }

        [Column("Company Name")]
        public string? CompanyName { get; set; }

        // Q1 - Q35 (including sub-questions)
        public string? Q1 { get; set; }
        public string? Q1A { get; set; }
        public string? Q2 { get; set; }
        public string? Q3 { get; set; }
        public string? Q3A { get; set; }
        public string? Q4 { get; set; }
        public string? Q5 { get; set; }
        public string? Q6 { get; set; }
        public string? Q7 { get; set; }
        public string? Q8 { get; set; }
        public string? Q9A { get; set; }
        public string? Q9B { get; set; }
        public string? Q9C { get; set; }
        public string? Q9D { get; set; }
        public string? Q9E { get; set; }
        public string? Q9F { get; set; }
        public string? Q9G { get; set; }
        public string? Q9H { get; set; }
        public string? Q9I { get; set; }
        public string? Q9J { get; set; }
        public string? Q9K { get; set; }
        public string? Q9L { get; set; }
        public string? Q9M { get; set; }
        public string? Q9N { get; set; }
        public string? Q10A { get; set; }
        public string? Q10B { get; set; }
        public string? Q10C { get; set; }
        public string? Q10D { get; set; }
        public string? Q10E { get; set; }
        public string? Q10F { get; set; }
        public string? Q10G { get; set; }
        public string? Q10H { get; set; }
        public string? Q10I { get; set; }
        public string? Q10J { get; set; }
        public string? Q10K { get; set; }
        public string? Q10L { get; set; }
        public string? Q10M { get; set; }
        public string? Q10N { get; set; }
        public string? Q11A { get; set; }
        public string? Q11B { get; set; }
        public string? Q11C { get; set; }
        public string? Q11D { get; set; }
        public string? Q11E { get; set; }
        public string? Q11F { get; set; }
        public string? Q12 { get; set; }
        public string? Q13 { get; set; }
        public string? Q14 { get; set; }
        public string? Q15 { get; set; }
        public string? Q16A { get; set; }
        public string? Q16B { get; set; }
        public string? Q16C { get; set; }
        public string? Q16D { get; set; }
        public string? Q16E { get; set; }
        public string? Q17A { get; set; }
        public string? Q17B { get; set; }
        public string? Q17C { get; set; }
        public string? Q17D { get; set; }
        public string? Q17E { get; set; }
        public string? Q17F { get; set; }
        public string? Q17G { get; set; }
        public string? Q17H { get; set; }
        public string? Q17I { get; set; }
        public string? Q18A { get; set; }
        public string? Q18B { get; set; }
        public string? Q18C { get; set; }
        public string? Q18D { get; set; }
        public string? Q18E { get; set; }
        public string? Q18F { get; set; }
        public string? Q18G { get; set; }
        public string? Q18H { get; set; }
        public string? Q18I { get; set; }
        public string? Q18J { get; set; }
        public string? Q18K { get; set; }
        public string? Q18L { get; set; }
        public string? Q18M { get; set; }
        public string? Q18N { get; set; }
        public string? Q19A { get; set; }
        public string? Q19B { get; set; }
        public string? Q19C { get; set; }
        public string? Q19D { get; set; }
        public string? Q19E { get; set; }
        public string? Q19F { get; set; }
        public string? Q19G { get; set; }
        public string? Q19H { get; set; }
        public string? Q19I { get; set; }
        public string? Q19J { get; set; }
        public string? Q19K { get; set; }
        public string? Q19L { get; set; }
        public string? Q19M { get; set; }
        public string? Q19N { get; set; }
        public string? Q20A { get; set; }
        public string? Q20B { get; set; }
        public string? Q20C { get; set; }
        public string? Q20D { get; set; }
        public string? Q20E { get; set; }
        public string? Q20F { get; set; }
        public string? Q20G { get; set; }
        public string? Q20H { get; set; }
        public string? Q20I { get; set; }
        public string? Q20J { get; set; }
        public string? Q20K { get; set; }
        public string? Q20L { get; set; }
        public string? Q20M { get; set; }
        public string? Q20N { get; set; }
        public string? Q20O { get; set; }
        public string? Q20P { get; set; }
        public string? Q21 { get; set; }
        public string? Q22A { get; set; }
        public string? Q22B { get; set; }
        public string? Q22C { get; set; }
        public string? Q22D { get; set; }
        public string? Q22E { get; set; }
        public string? Q22F { get; set; }
        public string? Q22G { get; set; }
        public string? Q22H { get; set; }
        public string? Q22I { get; set; }
        public string? Q22J { get; set; }
        public string? Q22K { get; set; }
        public string? Q22L { get; set; }
        public string? Q23A { get; set; }
        public string? Q23B { get; set; }
        public string? Q23C { get; set; }
        public string? Q23D { get; set; }
        public string? Q23E { get; set; }
        public string? Q24 { get; set; }
        public string? Q24A { get; set; }
        public string? Q25A { get; set; }
        public string? Q25B { get; set; }
        public string? Q25C { get; set; }
        public string? Q25D { get; set; }
        public string? Q25E { get; set; }
        public string? Q25F { get; set; }
        public string? Q25G { get; set; }
        public string? Q25H { get; set; }
        public string? Q26 { get; set; }
        public string? Q27 { get; set; }
        public string? Q28 { get; set; }
        public string? Q28A { get; set; }
        public string? Q29A { get; set; }
        public string? Q29B { get; set; }
        public string? Q29C { get; set; }
        public string? Q29D { get; set; }
        public string? Q29E { get; set; }
        public string? Q29F { get; set; }
        public string? Q30A { get; set; }
        public string? Q30B { get; set; }
        public string? Q30C { get; set; }
        public string? Q30D { get; set; }
        public string? Q30E { get; set; }
        public string? Q30F { get; set; }
        public string? Q30G { get; set; }
        public string? Q30H { get; set; }
        public string? Q30I { get; set; }
        public string? Q30J { get; set; }
        public string? Q31 { get; set; }
        public string? Q32A { get; set; }
        public string? Q32B { get; set; }
        public string? Q32C { get; set; }
        public string? Q32D { get; set; }
        public string? Q32E { get; set; }
        public string? Q32F { get; set; }
        public string? Q32G { get; set; }
        public string? Q32H { get; set; }
        public string? Q32I { get; set; }
        public string? Q32J { get; set; }
        public string? Q32K { get; set; }
        public string? Q32L { get; set; }
        public string? Q32M { get; set; }
        public string? Q32N { get; set; }
        public string? Q32O { get; set; }
        public string? Q32P { get; set; }
        public string? Q32Q { get; set; }
        public string? Q33A { get; set; }
        public string? Q33B { get; set; }
        public string? Q34A { get; set; }
        public string? Q34B { get; set; }
        public string? Q34C { get; set; }
        public string? Q34D { get; set; }
        public string? Q34E { get; set; }
        public string? Q34F { get; set; }
        public string? Q34G { get; set; }
        public string? Q34H { get; set; }
        public string? Q35 { get; set; }

    }
}

