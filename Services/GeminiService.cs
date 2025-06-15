using System.Text;
using System.Text.Json;
using MissionGenerator.Models;

namespace MissionGenerator.Services;

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Gemini:ApiKey"]
            ?? throw new ArgumentNullException("Gemini:ApiKey", "API key is missing.");
    }






    private string BuildPromptFromUserText(string userText)
    {
        return @$"
Tu es un assistant intelligent spécialisé dans la génération de fiches missions pour des plateformes de freelancing.

À partir d’une description utilisateur, tu dois générer un objet JSON avec les champs suivants , Donne-moi le résultat strictement au format JSON. **Pas de texte supplémentaire. Pas de balisage markdown (pas de ```json)**.
(sans aucun texte autour) :

{{
  ""title"": ""Titre concis"",
  ""description"": ""Description complète"",
  ""country"": ""Nom du pays en anglais"",
  ""city"": ""Ville"",
  ""workMode"": ""REMOTE, ONSITE, HYBRID"",
  ""duration"": nombre entier,
  ""durationType"": ""MONTH or YEAR"",
  ""startImmediately"": true/false,
  ""startDate"": ""yyyy-MM-dd or null"",
  ""experienceYear"": ""0-3, 3-7, 7-12, 12+"",
  ""contractType"": ""FORFAIT or REGIE"",
  ""estimatedDailyRate"": nombre entier en euros,
  ""domain"": ""secteur"",
  ""position"": ""poste"",
  ""requiredExpertises"": [""tech1"", ""tech2""]
}}

Description utilisateur : ""{userText}""
Donne-moi le résultat strictement au format JSON.
";
    }

    public async Task<Mission> GetMissionFromPromptAsync(string userText)
    {
        string prompt = BuildPromptFromUserText(userText);
var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception($"Gemini API error {response.StatusCode}: {err}");
        }

        var responseString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseString);
        var json = doc.RootElement
                      .GetProperty("candidates")[0]
                      .GetProperty("content")
                      .GetProperty("parts")[0]
                      .GetProperty("text")
                      .GetString();

        if (string.IsNullOrWhiteSpace(json))
            throw new Exception("Gemini returned empty or null content");

        try
        {
            Console.WriteLine("🔍 Raw Gemini Response:");
            Console.WriteLine(json);

// Remove markdown code block formatting if present
var cleaned = json
    .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
    .Replace("```", "", StringComparison.OrdinalIgnoreCase)
    .Trim();

// Find first and last braces
var startIndex = cleaned.IndexOf('{');
var endIndex = cleaned.LastIndexOf('}');
if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
    throw new Exception("No valid JSON object found in Gemini response.");

// Extract only the JSON object
var jsonOnly = cleaned.Substring(startIndex, endIndex - startIndex + 1);
Console.WriteLine("✅ Cleaned JSON:");
Console.WriteLine(jsonOnly);

return JsonSerializer.Deserialize<Mission>(jsonOnly)!;
        }
        catch (JsonException ex)
        {
            Console.WriteLine("❌ Failed to deserialize Gemini response:");
            Console.WriteLine(json);
            throw new Exception("Invalid JSON returned by Gemini", ex);
        }
    }
}
