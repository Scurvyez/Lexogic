using System.Text.Json;

namespace Lexogic
{
    public class DictionaryService
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "1785f668-ba89-4a18-a90d-1b59082d6124";
        private const string DictionaryReference = "collegiate";

        public DictionaryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetDefinitionAsync(string word)
        {
            string requestUri = $"https://dictionaryapi.com/api/v3/references/{DictionaryReference}/json/{Uri.EscapeDataString(word)}?key={ApiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
                return "Failed to retrieve a definition.";

            string jsonContent = await response.Content.ReadAsStringAsync();
            JsonDocument jsonResponse = JsonDocument.Parse(jsonContent);

            if (jsonResponse.RootElement.ValueKind != JsonValueKind.Array || jsonResponse.RootElement.GetArrayLength() == 0)
            {
                return "No definition(s) found or entry is not a standard dictionary entry.";
            }

            JsonElement firstEntry = jsonResponse.RootElement[0];

            if (firstEntry.ValueKind == JsonValueKind.String)
            {
                return "No exact match found. Did you mean: " + string.Join(", ", jsonResponse.RootElement.EnumerateArray().Select(e => e.GetString()));
            }

            if (!firstEntry.TryGetProperty("shortdef", out JsonElement shortDefArray) || shortDefArray.GetArrayLength() == 0)
            {
                return "No definition(s) found or entry is not a standard dictionary entry.";
            }

            var definitions = shortDefArray.EnumerateArray()
                                           .Select((def, index) => $"{index + 1}. {def.GetString()}");

            string allDefinitions = string.Join("\n", definitions);

            return $"\n{allDefinitions}";
        }
        
        public async Task<string> GetVariantsAsync(string word)
        {
            string requestUri = $"https://dictionaryapi.com/api/v3/references/{DictionaryReference}/json/{Uri.EscapeDataString(word)}?key={ApiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
                return "Failed to retrieve any variant spelling(s).";

            string jsonContent = await response.Content.ReadAsStringAsync();
            JsonDocument jsonResponse = JsonDocument.Parse(jsonContent);

            if (jsonResponse.RootElement.ValueKind != JsonValueKind.Array || jsonResponse.RootElement.GetArrayLength() == 0)
            {
                return "No variant(s) found or entry is not a standard dictionary entry.";
            }

            JsonElement firstEntry = jsonResponse.RootElement[0];

            if (firstEntry.ValueKind == JsonValueKind.String)
            {
                return "No exact match found. Did you mean: " + string.Join(", ", jsonResponse.RootElement.EnumerateArray().Select(e => e.GetString()));
            }

            if (!firstEntry.TryGetProperty("vrs", out JsonElement variantsArray) || variantsArray.GetArrayLength() == 0)
            {
                return "No variant spelling(s) found for this word.";
            }

            var variants = variantsArray.EnumerateArray()
                .Select(v => v.GetProperty("va").GetString());

            string allVariants = string.Join(", ", variants);

            return $": {allVariants}";
        }
    }
}