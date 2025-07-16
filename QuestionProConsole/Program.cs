using System;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using QuestionProConsole.Context;
using QuestionProConsole.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

class Program
{
    static async Task Main(string[] args)
    {
        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Create DbContextOptions with the connection string
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));


        string apiKey = "a4c91437-8412-4477-a079-a00b0b4924eb";
        string surveyId = "12895659";

        string connectionString = configuration.GetConnectionString("DefaultConnection");

        //string questionsApiUrl = $"https://api.questionpro.com/a/api/v2/surveys/{surveyId}/questions?page=1&perPage=100";
        //var questionSurveyData = await FetchQuestionsData(questionsApiUrl, apiKey);
        //Console.WriteLine(questionSurveyData);

        // Insert columns from QuestionsPro to QuestionsLookup
        //var stat = insertColumnsAsync(connectionString);

        Console.WriteLine("completed");
        // Fetch survey data
        string responsesApiUrl = $"https://api.questionpro.com/a/api/v2/surveys/{surveyId}/responses?page=1&perPage=100";
        var responsesSurveyData = await FetchSurveyData(responsesApiUrl, apiKey);

        if (responsesSurveyData != null)
        {
            // Save to database using DbContext
            using (var context = new ApplicationDbContext(optionsBuilder.Options))
            {
                Console.WriteLine(context);
                await SaveSurveyToDatabase(context, responsesSurveyData);
            }
        }
        else
        {
            Console.WriteLine("Failed to fetch survey data.");
        }
    }

    static async Task<QuestionProResponseDto> FetchSurveyData(string apiUrl, string apiKey)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            try
            {
                HttpResponseMessage res = await client.GetAsync(apiUrl);
                 if (res.IsSuccessStatusCode)
                {
                    string json = await res.Content.ReadAsStringAsync();
                    JObject jsonObject = JObject.Parse(json);
                    JToken responseToken = jsonObject["response"];
                    //if (responseToken != null && responseToken.Type == JTokenType.Array)
                    //{
                    //    foreach (var responseItem in responseToken)
                    //    {
                    //        Console.WriteLine("➡️ Response Item:");
                    //        Console.WriteLine(responseItem);
                    //        foreach (var prop in responseItem.Children<JProperty>())
                    //        {
                    //            Console.WriteLine($"{prop.Name}: {prop.Value}");
                    //        }
                    //        Console.WriteLine("---------------");
                    //    }
                    //}
                    //else
                    //{
                    //    Console.WriteLine("❌ 'response' is not an array or not found.");
                    //}
                    var responses = responseToken.ToObject<List<Results>>();
                    //var completedResponses = responses
                    //    .Where(r => r.responseStatus == "Completed")
                    //    .ToList();
                    var dto = new QuestionProResponseDto
                    {
                        response = responses
                    };
                    return dto;
                }
                else
                {
                    Console.WriteLine($"Failed to get survey data. Status: {res.StatusCode}");
                    string error = await res.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred while fetching survey data: {ex.Message}");
            }
        }

        return null;
    }

    static async Task<QuestionProResponseDto> FetchQuestionsData(string apiUrl, string apiKey)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            try
            {
                HttpResponseMessage res = await client.GetAsync(apiUrl);
                if (res.IsSuccessStatusCode)
                {
                    string json = await res.Content.ReadAsStringAsync();
                    JObject jsonObject = JObject.Parse(json);
                    JToken responseToken = jsonObject["response"];
                    if (responseToken != null && responseToken.Type == JTokenType.Array)
                    {
                        foreach (var responseItem in responseToken)
                        {
                            Console.WriteLine("➡️ Response Item:");
                            Console.WriteLine(responseItem);
                            Console.WriteLine("---------------");
                        }
                    }
                    else
                    {
                        Console.WriteLine("❌ 'response' is not an array or not found.");
                    }
                    //var responses = responseToken.ToObject<List<Results>>();
                    //var dto = new QuestionProResponseDto
                    //{
                    //    response = responses
                    //};
                    return null;
                }
                else
                {
                    Console.WriteLine($"Failed to get survey data. Status: {res.StatusCode}");
                    string error = await res.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred while fetching survey data: {ex.Message}");
            }
        }

        return null;
    }

    static async Task SaveSurveyToDatabase(ApplicationDbContext context, QuestionProResponseDto surveyData)
    {
        if (surveyData?.response == null || surveyData.response.Count() == 0)
        {
            Console.WriteLine("No responses to save.");
            return;
        }
        var allLookups = context.QuestionsLookups.ToList();
        foreach (var response in surveyData.response)
        {
            if (response != null)
            {
                string responseId = response != null ? response.responseID.ToString() : "";
                //if (responseId == "176795121")
                //{
                //    Console.WriteLine("Last record db");
                //}
                // Check if ResponseID already exists
                bool exists = await context.TuSurveyQuestions
                                            .AnyAsync(q => q.ResponseID == responseId);
                if (exists)
                {
                    Console.WriteLine(responseId + " - already exist.");
                }
                else
                {
                    string cleaned = string.Join(" ", response.timestamp.Split(' ').Take(5));

                    DateTime parsed = DateTime.ParseExact(cleaned, "dd MMM, yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                    var survey = new QuestionsPro
                    {
                        ResponseID = response != null ? response.responseID.ToString() : "",
                        ResponseStatus = response.responseStatus.ToString(),
                        IPAddress = response.ipAddress,
                        Timestamp = parsed,
                        Duplicate = response.duplicate.ToString(),
                        TimeTakenToComplete = response.timeTaken,
                        ExternalReference = "",
                        CustomVariable1 = response.customVariables?.custom1?.ToString() ?? "",
                        CustomVariable2 = response.customVariables?.custom2?.ToString() ?? "",
                        CustomVariable3 = response.customVariables?.custom3?.ToString() ?? "",
                        CustomVariable4 = response.customVariables?.custom4?.ToString() ?? "",
                        CustomVariable5 = response.customVariables?.custom5?.ToString() ?? "",
                        CountryCode = response.location.countryCode,
                        Region = response.location.region,
                    };
                    survey.FirstName = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "FirstName"));
                    survey.LastName = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "LastName"));
                    survey.EmailAddress = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "EmailAddress"));
                    survey.CompanyName = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "CompanyName"));
                    survey.Q1 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q1"));
                    survey.Q1A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q1A"));
                    survey.Q2 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q2"));
                    survey.Q3 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q3"));
                    survey.Q3A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q3A"));
                    survey.Q4 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q4"));
                    survey.Q5 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q5"));
                    survey.Q6 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q6"));
                    survey.Q7 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q7"));
                    survey.Q8 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q8"));
                    survey.Q9A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q9A"));
                    survey.Q9B = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q9B"));
                    survey.Q9C = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q9C"));
                    survey.Q9D = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q9D"));
                    survey.Q9E = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q9E"));
                    survey.Q9F = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q9F"));
                    survey.Q9G = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q9G"));
                    survey.Q9H = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q9H"));
                    survey.Q9I = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q9I"));
                    survey.Q9J = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q9J"));
                    survey.Q9K = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q9K"));
                    survey.Q9L = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q9L"));
                    survey.Q9M = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q9M"));
                    survey.Q9N = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q9N"));
                    survey.Q10A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q10A"));
                    survey.Q10B = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q10B"));
                    survey.Q10C = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q10C"));
                    survey.Q10D = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q10D"));
                    survey.Q10E = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q10E"));
                    survey.Q10F = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q10F"));
                    survey.Q10G = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q10G"));
                    survey.Q10H = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q10H"));
                    survey.Q10I = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q10I"));
                    survey.Q10J = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q10J"));
                    survey.Q10K = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q10K"));
                    survey.Q10L = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q10L"));
                    survey.Q10M = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q10M"));
                    survey.Q10N = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q10N"));
                    survey.Q11A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q11A"));
                    survey.Q11B = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q11B"));
                    survey.Q11C = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q11C"));
                    survey.Q11D = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q11D"));
                    survey.Q11E = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q11E"));
                    survey.Q11F = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q11F"));
                    survey.Q12 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q12"));
                    survey.Q13 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q13"));
                    survey.Q14 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q14"));
                    survey.Q15 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q15"));
                    survey.Q16A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q16A"));
                    survey.Q16B = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q16B"));
                    survey.Q16C = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q16C"));
                    survey.Q16D = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q16D"));
                    survey.Q16E = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q16E"));
                    survey.Q17A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q17A"));
                    survey.Q17B = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q17B"));
                    survey.Q17C = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q17C"));
                    survey.Q17D = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q17D"));
                    survey.Q17E = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q17E"));
                    survey.Q17F = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q17F"));
                    survey.Q17G = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q17G"));
                    survey.Q17H = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q17H"));
                    survey.Q17I = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q17I"));
                    survey.Q18A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q18A"));
                    survey.Q18B = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q18B"));
                    survey.Q18C = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q18C"));
                    survey.Q18D = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q18D"));
                    survey.Q18E = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q18E"));
                    survey.Q18F = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q18F"));
                    survey.Q18G = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q18G"));
                    survey.Q18H = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q18H"));
                    survey.Q18I = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q18I"));
                    survey.Q18J = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q18J"));
                    survey.Q18K = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q18K"));
                    survey.Q18L = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q18L"));
                    survey.Q18M = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q18M"));
                    survey.Q18N = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q18N"));
                    survey.Q19A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q19A"));
                    survey.Q19B = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q19B"));
                    survey.Q19C = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q19C"));
                    survey.Q19D = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q19D"));
                    survey.Q19E = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q19E"));
                    survey.Q19F = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q19F"));
                    survey.Q19G = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q19G"));
                    survey.Q19H = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q19H"));
                    survey.Q19I = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q19I"));
                    survey.Q19J = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q19J"));
                    survey.Q19K = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q19K"));
                    survey.Q19L = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q19L"));
                    survey.Q19M = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q19M"));
                    survey.Q19N = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q19N"));
                    survey.Q20A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q20A"));
                    survey.Q20B = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q20B"));
                    survey.Q20C = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q20C"));
                    survey.Q20D = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q20D"));
                    survey.Q20E = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q20E"));
                    survey.Q20F = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q20F"));
                    survey.Q20G = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q20G"));
                    survey.Q20H = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q20H"));
                    survey.Q20I = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q20I"));
                    survey.Q20J = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q20J"));
                    survey.Q20K = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q20K"));
                    survey.Q20L = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q20L"));
                    survey.Q20M = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q20M"));
                    survey.Q20N = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q20N"));
                    survey.Q20O = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q20O"));
                    survey.Q20P = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q20P"));
                    survey.Q21 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q21"));
                    survey.Q22A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q22A"));
                    survey.Q22B = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q22B"));
                    survey.Q22C = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q22C"));
                    survey.Q22D = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q22D"));
                    survey.Q22E = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q22E"));
                    survey.Q22F = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q22F"));
                    survey.Q22G = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q22G"));
                    survey.Q22H = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q22H"));
                    survey.Q22I = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q22I"));
                    survey.Q22J = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q22J"));
                    survey.Q22K = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q22K"));
                    survey.Q22L = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q22L"));
                    survey.Q23A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q23A"));
                    survey.Q23B = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q23B"));
                    survey.Q23C = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q23C"));
                    survey.Q23D = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q23D"));
                    survey.Q23E = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q23E"));
                    survey.Q24 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q24"));
                    survey.Q24A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q24A"));
                    survey.Q25A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q25A"));
                    survey.Q25B = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q25B"));
                    survey.Q25C = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q25C"));
                    survey.Q25D = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q25D"));
                    survey.Q25E = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q25E"));
                    survey.Q25F = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q25F"));
                    survey.Q25G = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q25G"));
                    survey.Q25H = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q25H"));
                    survey.Q26 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q26"));
                    survey.Q27 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q27"));
                    survey.Q28 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q28"));
                    survey.Q28A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q28A"));
                    survey.Q29A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q29A"));
                    survey.Q29B = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q29B"));
                    survey.Q29C = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q29C"));
                    survey.Q29D = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q29D"));
                    survey.Q29E = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q29E"));
                    survey.Q29F = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q29F"));
                    survey.Q30A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q30A"));
                    survey.Q30B = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q30B"));
                    survey.Q30C = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q30C"));
                    survey.Q30D = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q30D"));
                    survey.Q30E = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q30E"));
                    survey.Q30F = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q30F"));
                    survey.Q30G = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q30G"));
                    survey.Q30H = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q30H"));
                    survey.Q30I = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q30I"));
                    survey.Q30J = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q30J"));
                    survey.Q31 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q31"));
                    survey.Q32A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q32A"));
                    survey.Q32B = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q32B"));
                    survey.Q32C = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q32C"));
                    survey.Q32D = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q32D"));
                    survey.Q32E = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q32E"));
                    survey.Q32F = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q32F"));
                    survey.Q32G = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q32G"));
                    survey.Q32H = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q32H"));
                    survey.Q32I = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q32I"));
                    survey.Q32J = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q32J"));
                    survey.Q32K = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q32K"));
                    survey.Q32L = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q32L"));
                    survey.Q32M = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q32M"));
                    survey.Q32N = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q32N"));
                    survey.Q32O = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q32O"));
                    survey.Q32P = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q32P"));
                    survey.Q32Q = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q32Q"));
                    survey.Q33A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q33A"));
                    survey.Q33B = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q33B"));
                    survey.Q34A = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q34A"));
                    survey.Q34B = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q34B"));
                    survey.Q34C = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q34C"));
                    survey.Q34D = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q34D"));
                    survey.Q34E = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q34E"));
                    survey.Q34F = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q34F"));
                    survey.Q34G = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q34G"));
                    survey.Q34H = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q34H"));
                    survey.Q35 = GetAnswerValuesByQuestionCode(response.responseSet, GetFirstLookupByTargetColumn(allLookups, "Q35"));
                    Console.WriteLine(JsonConvert.SerializeObject(survey, Formatting.Indented));
                    context.TuSurveyQuestions.Add(survey);
                }
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine("Survey data saved to database using DbContext.");
    }

    public static string? GetAnswerValuesByQuestionCode(List<ResponseSet> questions, QuestionsLookup questionLookup)
    {
        if (questions == null)
            return null;

        // Find the matching response question by ID
        var responseQuestion = questions
            .FirstOrDefault(q => q.questionID == questionLookup.QuestionId);

        if (responseQuestion.answerValues == null || !responseQuestion.answerValues.Any())
            return null;

        Console.WriteLine(responseQuestion?.answerValues.ToString());
        //string answerText = responseQuestion?.answerValues?.FirstOrDefault()?.answerText;

        // If specific AnswerId is provided, find only that answer
        if (questionLookup.AnswerId.HasValue)
        {
            var match = responseQuestion.answerValues
                .FirstOrDefault(av => av.answerID == questionLookup.AnswerId.Value);

            if (match != null)
            {
                var valueText = match.value?.text?.Trim();
                if (!string.IsNullOrWhiteSpace(valueText))
                    return valueText;

                var answerText = match.answerText?.Trim();
                return !string.IsNullOrWhiteSpace(answerText) ? answerText : null;
            }

            return null; // No match found for the AnswerId
        }
        else
        {
            var mergedText = "";
            //if (questionLookup.QuestionCode == "Q2_1" || questionLookup.QuestionCode == "Q2_2" || questionLookup.QuestionCode == "Q2_3" || questionLookup.QuestionCode == "Q2_4")
            //{
            //    mergedText = questionLookup.AnswerId;
            //} else
            //{
                mergedText = string.Join(", ",
                responseQuestion.answerValues
                    .Select(av =>
                    {
                        var valueText = av.value?.text?.Trim();
                        if (!string.IsNullOrWhiteSpace(valueText))
                            return valueText;
                        var validCodes = new[] { "Q2_1", "Q2_2", "Q2_3", "Q2_4" };
                        if (validCodes.Contains(questionLookup.QuestionCode))
                        {
                            return "";
                        } else
                        {
                            var answerText = av.answerText?.Trim();
                            return !string.IsNullOrWhiteSpace(answerText) ? answerText : null;
                        }
                    })
                    .Where(text => !string.IsNullOrWhiteSpace(text)));
            //}

            if(questionLookup.TargetColumn == "Q21")
            {
                return string.IsNullOrEmpty(mergedText) ? null : mergedText;
            } else
            {
                return string.IsNullOrEmpty(mergedText) ? null : mergedText;
            }
            
        }
        //string? answerText = responseQuestion?.answerValues != null
        //    ? string.Join(", ", responseQuestion.answerValues.Select(a => a.answerText?.Trim()).Where(t => !string.IsNullOrEmpty(t)))
        //    : null;
        //return answerText;
    }

    public static async Task<string?> insertColumnsAsync(String connectionString)
    {
        var columnNames = new List<string>();

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            var command = new SqlCommand(
                @"SELECT COLUMN_NAME 
                  FROM INFORMATION_SCHEMA.COLUMNS 
                  WHERE TABLE_NAME = 'QuestionsPro' 
                  AND COLUMN_NAME LIKE 'Q%'", connection);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    columnNames.Add(reader.GetString(0));
                }
            }
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        Console.WriteLine("Columns fetched from database:");
        using (var context = new ApplicationDbContext(optionsBuilder.Options))
        {
            Console.WriteLine(context);
            foreach (var col in columnNames)
            {
                Console.WriteLine(col);
                var newEntry = new QuestionsLookup
                {
                    TargetColumn = col
                    // You can set other properties as needed
                };

                context.QuestionsLookups.Add(newEntry);
            }
            context.SaveChanges();
        }
        return null;
    }

    public static QuestionsLookup GetFirstLookupByTargetColumn(List<QuestionsLookup> allLookups, string targetColumn)
    {
        var firstLookup = allLookups
            .FirstOrDefault(l => l.TargetColumn == targetColumn);

        return firstLookup;
    }
}