using Discord.WebSocket;

namespace Lexogic
{
    public class CommandHandler
    {
        private readonly DictionaryService _dictionaryService;

        public CommandHandler(DictionaryService dictionaryService)
        {
            _dictionaryService = dictionaryService;
        }

        public async Task HandleSlashCommandAsync(SocketSlashCommand command)
        {
            string word = command.Data.Options.First().Value.ToString() ?? string.Empty;
            
            switch (command.CommandName)
            {
                case "define":
                    await HandleDictionaryCommandAsync(command, word, _dictionaryService.GetDefinitionAsync, _dictionaryService.IsWordOffensiveAsync);
                    break;
                
                case "variants":
                    await HandleDictionaryCommandAsync(command, word, _dictionaryService.GetVariantsAsync, _dictionaryService.IsWordOffensiveAsync);
                    break;
                
                case "etymology":
                    await HandleDictionaryCommandAsync(command, word, _dictionaryService.GetEtymologyAsync, _dictionaryService.IsWordOffensiveAsync);
                    break;

                default:
                    await command.RespondAsync("Unknown command.");
                    break;
            }
        }

        private static async Task HandleDictionaryCommandAsync(SocketSlashCommand command, string word, 
            Func<string, Task<string>> getInfoAsync, Func<string, Task<bool>> isWordOffensiveAsync)
        {
            bool isOffensive = await isWordOffensiveAsync(word);
            string response = await getInfoAsync(word);

            string message = isOffensive
                ? "The word is deemed offensive and cannot be displayed."
                : $"**`{word}...`** {response}";

            await command.RespondAsync(message);
        }
    }
}