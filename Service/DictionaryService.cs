using System.Text.Json;

namespace Lexogic
{
    public class DictionaryService
    {
        private readonly HttpClient _httpClient;
        private const string DictionaryReference = "collegiate";
        private readonly string _mW_Api_Key;
        
        public DictionaryService(HttpClient httpClient)
        {
            _mW_Api_Key = Environment.GetEnvironmentVariable("MW_API_KEY") ?? string.Empty;
            _httpClient = httpClient;
        }

        public async Task<bool> IsWordOffensiveAsync(string word)
        {
            JsonElement? firstEntry = await FetchWordEntryAsync(word);

            if (firstEntry == null)
                return false;

            if (firstEntry.Value.TryGetProperty("meta", out JsonElement meta) &&
                meta.TryGetProperty("offensive", out JsonElement offensive))
            {
                return offensive.GetBoolean();
            }

            return false;
        }

        public async Task<string> GetDefinitionAsync(string word)
        {
            JsonElement? firstEntry = await FetchWordEntryAsync(word);

            if (firstEntry == null)
                return "\n**`Failed to retrieve any definitions.`**";

            string stems = "";
            if (firstEntry.Value.TryGetProperty("meta", out JsonElement meta) &&
                meta.TryGetProperty("stems", out JsonElement stemsArray))
            {
                stems = $"\n**`Related Terms:`** {string.Join(", ", stemsArray.EnumerateArray().Select(stem => stem.GetString()))}";
            }

            if (firstEntry.Value.TryGetProperty("suggestions", out JsonElement suggestions))
            {
                return $"\n{suggestions.GetString()}";
            }
    
            string definition = ParseDefinitionsBySense(firstEntry.Value, "definition(s)");
            return $"{definition}{stems}";
        }

        public async Task<string> GetVariantsAsync(string word)
        {
            JsonElement? firstEntry = await FetchWordEntryAsync(word);
            
            if (firstEntry == null)
                return "\n**`Failed to retrieve any variant spelling(s).`**";

            return firstEntry.Value.TryGetProperty("suggestions", out JsonElement suggestions) 
                ? $"\n{suggestions.GetString()}" 
                : ParseVariants(firstEntry.Value);
        }

        public async Task<string> GetEtymologyAsync(string word)
        {
            JsonElement? firstEntry = await FetchWordEntryAsync(word);
            
            if (firstEntry == null)
                return "\n**`Failed to retrieve any known etymologies.`**";

            return firstEntry.Value.TryGetProperty("suggestions", out JsonElement suggestions) 
                ? $"\n{suggestions.GetString()}" 
                : ParseEtymologies(firstEntry.Value);
        }
        
        public async Task<string> GetSynonymsAsync(string word)
        {
            JsonElement? firstEntry = await FetchWordEntryAsync(word);

            if (firstEntry == null)
                return "\n**`Failed to retrieve any synonyms.`**";

            return firstEntry.Value.TryGetProperty("syns", out JsonElement synsArray) 
                ? $"\n{SynonymExtractor.ExtractSynonyms(synsArray)}" 
                : "\n**`No synonyms found for this word.`**";
        }
        
        private async Task<JsonElement?> FetchWordEntryAsync(string word)
        {
            string requestUri = $"https://dictionaryapi.com/api/v3/references/{DictionaryReference}/json/{Uri.EscapeDataString(word)}?key={_mW_Api_Key}";
            HttpResponseMessage response = await _httpClient.GetAsync(requestUri);
            
            if (!response.IsSuccessStatusCode)
                return null;

            string jsonContent = await response.Content.ReadAsStringAsync();
            JsonDocument jsonResponse = JsonDocument.Parse(jsonContent);

            if (jsonResponse.RootElement.ValueKind != JsonValueKind.Array || jsonResponse.RootElement.GetArrayLength() == 0)
                return null;

            JsonElement firstEntry = jsonResponse.RootElement[0];

            if (firstEntry.ValueKind == JsonValueKind.Object)
            {
                return firstEntry;
            }

            if (firstEntry.ValueKind != JsonValueKind.String) return null;
            var suggestions = jsonResponse.RootElement.EnumerateArray()
                .Select(e => e.GetString())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            if (suggestions.Count <= 0) return null;
            string suggestionMessage = string.Join(", ", suggestions);
            return JsonDocument.Parse($"{{\"suggestions\": \"**`No exact match found. Did you mean:`** {suggestionMessage}\"}}").RootElement;
        }
        
        /// <summary>
        /// OBSOLETE
        /// </summary>
        private static string ParseShortDefinitions(JsonElement entry, string notFoundMessage)
        {
            if (!entry.TryGetProperty("shortdef", out JsonElement shortDefArray) || shortDefArray.GetArrayLength() == 0)
                return $"\nNo {notFoundMessage} found or entry is not a standard dictionary entry.";

            var definitions = shortDefArray.EnumerateArray()
                .Select((def, index) => $"**`{index + 1}.`** {JSONParserHelper.ParseJSONTags(def.GetString() ?? string.Empty)}");

            return $"\n{string.Join("\n", definitions)}";
        }
        
        private static string ParseDefinitionsBySense(JsonElement entry, string notFoundMessage)
        {
            if (!entry.TryGetProperty("def", out JsonElement defArray) || defArray.GetArrayLength() == 0)
                return $"\nNo {notFoundMessage} found or entry is not a standard dictionary entry.";

            List<string> sensesList = new List<string>();
    
            foreach (JsonElement defElement in defArray.EnumerateArray())
            {
                if (!defElement.TryGetProperty("sseq", out JsonElement sseqArray)) continue;
                foreach (JsonElement senseSequence in sseqArray.EnumerateArray())
                {
                    foreach (JsonElement senseGroup in senseSequence.EnumerateArray())
                    {
                        if (senseGroup[0].GetString() != "sense") continue;
                        JsonElement senseDetails = senseGroup[1];
                        string partOfSpeech = entry.TryGetProperty("fl", out JsonElement flElement) ? flElement.GetString() ?? "Unknown" : "Unknown";
                        string senseDefinition = ParseShortDefinitions(senseDetails, "definition(s)");
                        sensesList.Add($"**`{partOfSpeech.ToUpper()}`**:\n{senseDefinition}");
                    }
                }
            }
            return sensesList.Any() ? string.Join("\n", sensesList) : $"\nNo {notFoundMessage} found.";
        }

        private static string ParseVariants(JsonElement entry)
        {
            if (!entry.TryGetProperty("vrs", out JsonElement variantsArray) || variantsArray.GetArrayLength() == 0)
                return "\n**`No variant spelling(s) found for this word.`**";

            var variants = variantsArray.EnumerateArray()
                .Select(v => JSONParserHelper.ParseJSONTags(v.GetProperty("va").GetString() ?? string.Empty));

            return $"\n{string.Join(", ", variants)}";
        }

        private static string ParseEtymologies(JsonElement entry)
        {
            if (!entry.TryGetProperty("et", out JsonElement etymologyArray) || etymologyArray.ValueKind != JsonValueKind.Array)
            {
                return "\n**`No known etymology found for this word.`**";
            }

            var etymologies = new List<string>();

            foreach (JsonElement et in etymologyArray.EnumerateArray())
            {
                if (et.ValueKind == JsonValueKind.Array && et.GetArrayLength() == 2)
                {
                    JsonElement typeElement = et[0];
                    JsonElement textElement = et[1];

                    if (typeElement.ValueKind == JsonValueKind.String && typeElement.GetString() == "text" &&
                        textElement.ValueKind == JsonValueKind.String)
                    {
                        string formattedText = JSONParserHelper.ParseJSONTags(textElement.GetString() ?? string.Empty);
                        etymologies.Add($"**`{etymologies.Count + 1}.`** {formattedText}");
                    }
                    else
                    {
                        etymologies.Add($"**`{etymologies.Count + 1}.`** [Invalid etymology data]");
                    }
                }
                else
                {
                    etymologies.Add("[Invalid etymology data]");
                }
            }
            string allEtymologies = string.Join("\n", etymologies);
            
            return $"\n{allEtymologies}";
        }
        
        private static string ParseSynonyms(JsonElement synsArray)
        {
            List<string> synonymSections = new List<string>();

            foreach (JsonElement synSection in synsArray.EnumerateArray())
            {
                if (!synSection.TryGetProperty("pl", out JsonElement label) ||
                    !synSection.TryGetProperty("pt", out JsonElement ptArray)) continue;
                string sectionLabel = $"**`{JSONParserHelper.ParseJSONTags(label.GetString() ?? string.Empty)}`**:";
                var textParts = new List<string>();

                foreach (JsonElement ptElement in ptArray.EnumerateArray())
                {
                    if (ptElement[0].GetString() == "text")
                    {
                        textParts.Add(JSONParserHelper.ParseJSONTags(ptElement[1].GetString() ?? string.Empty));
                    }
                }
                synonymSections.Add($"{sectionLabel}\n{string.Join("\n", textParts)}");
            }

            return string.Join("\n", synonymSections);
        }
    }
}