using System.Text.Json;
using Discord;
using Discord.WebSocket;

namespace Lexogic
{
    public class Program
    {
        private DiscordSocketClient _client;
        private static readonly HttpClient httpClient = new ();
        private const string ApiKey = "1785f668-ba89-4a18-a90d-1b59082d6124";
        private const string DictionaryReference = "collegiate";
        
        public static Task Main(string[] args) => new Program().MainAsync();

        private async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _client.Log += LogAsync;
            _client.MessageReceived += MessageReceivedAsync;

            string token = GetTokenFromFile("token.txt");

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Error: Bot token not found. Ensure 'token.txt' contains the token.");
                return;
            }
            
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }
        
        private static string GetTokenFromFile(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath).Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading the token file: {ex.Message}");
                return string.Empty;
            }
        }
        
        private static Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log);
            return Task.CompletedTask;
        }
        
        private static async Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.IsBot) return;

            if (message.Content.StartsWith("!define "))
            {
                string word = message.Content.Substring(8).Trim();
                string definition = await GetDefinitionAsync(word);

                if (string.IsNullOrEmpty(definition))
                {
                    await message.Channel.SendMessageAsync($"No definitions found for **{word}**.");
                }
                else
                {
                    await message.Channel.SendMessageAsync($"**{word}**: {definition}");
                }
            }
        }

        private static async Task<string> GetDefinitionAsync(string word)
        {
            // Construct the API request URL
            string requestUri = $"https://dictionaryapi.com/api/v3/references/{DictionaryReference}/json/{Uri.EscapeDataString(word)}?key={ApiKey}";

            HttpResponseMessage response = await httpClient.GetAsync(requestUri);

            if (response.IsSuccessStatusCode)
            {
                string jsonContent = await response.Content.ReadAsStringAsync();

                // Parse the JSON response
                JsonDocument jsonResponse = JsonDocument.Parse(jsonContent);
                if (jsonResponse.RootElement.ValueKind == JsonValueKind.Array && jsonResponse.RootElement.GetArrayLength() > 0)
                {
                    JsonElement firstEntry = jsonResponse.RootElement[0];

                    // Check if it's a dictionary entry with "shortdef"
                    if (firstEntry.TryGetProperty("shortdef", out JsonElement shortDefArray))
                    {
                        string definition = shortDefArray[0].GetString();
                        return definition;
                    }
                    else
                    {
                        // Handle case where no definition is found or it's not a standard dictionary entry
                        return "No definition found or entry is not a standard dictionary entry.";
                    }
                }
            }

            return "Failed to retrieve the definition.";
        }
    }
}