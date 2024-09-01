using System.Text.Json;
using Discord;
using Discord.WebSocket;
using RestSharp;

namespace Lexogic
{
    /// <summary>
    /// Renew the server host at https://client.pylexnodes.net
    /// </summary>
    public class Program
    {
        private DiscordSocketClient _client;
        
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
            RestClient client = new ("https://api.dictionaryapi.dev/api/v2/entries/en/");
            RestRequest request = new (word);
            RestResponse response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                // Parse the JSON response and extract the definition
                JsonDocument jsonResponse = JsonDocument.Parse(response.Content ?? string.Empty);
                JsonElement firstMeaning = jsonResponse.RootElement[0].GetProperty("meanings")[0];
                JsonElement firstDefinition = firstMeaning.GetProperty("definitions")[0];
                return firstDefinition.GetProperty("definition").GetString() ?? string.Empty;
            }
            return null!;
        }
    }
}