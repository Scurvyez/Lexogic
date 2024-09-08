using System.Text.Json;

namespace Lexogic
{
    public static class SynonymExtractor
    {
        public static string ExtractSynonyms(JsonElement synsArray)
        {
            var synonymsList = new List<string>();
            foreach (JsonElement synSection in synsArray.EnumerateArray())
            {
                foreach (JsonElement ptElement in synSection.GetProperty("pt").EnumerateArray())
                {
                    if (ptElement[0].GetString() != "text") continue;
                    string text = JSONParserHelper.ParseJSONTags(ptElement[1].GetString() ?? string.Empty);

                    var synonyms = ExtractSynonymsFromText(text);
                    synonymsList.AddRange(synonyms);
                    break;
                }
            }
            return synonymsList.Any() ? string.Join(", ", synonymsList) : "**`No synonyms available.`**";
        }

        private static IEnumerable<string> ExtractSynonymsFromText(string text)
        {
            var result = new List<string>();
            string[] words = text.Split(' ');

            foreach (string word in words)
            {
                if (word.Contains("mean", StringComparison.OrdinalIgnoreCase) ||
                    word.Contains("implies", StringComparison.OrdinalIgnoreCase) ||
                    word.Contains("used", StringComparison.OrdinalIgnoreCase) ||
                    word.Contains('.'))
                {
                    break;
                }

                string cleanedWord = word.Trim('{', '}', '\\', '/', ',', '.');
                if (!string.IsNullOrWhiteSpace(cleanedWord))
                {
                    result.Add(cleanedWord);
                }
            }
            return (result.Any() ? result : null)!;
        }
    }
}