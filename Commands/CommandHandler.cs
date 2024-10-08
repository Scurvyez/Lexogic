using Discord.WebSocket;

namespace Lexogic
{
    public class CommandHandler
    {
        private readonly DictionaryService _dictionaryService;
        private readonly InstanceInfo _instanceInfo;

        public CommandHandler(DictionaryService dictionaryService, InstanceInfo instanceInfo)
        {
            _dictionaryService = dictionaryService;
            _instanceInfo = instanceInfo;
        }

        public async Task HandleSlashCommandAsync(SocketSlashCommand command)
        {
            string? word = command.Data.Options.FirstOrDefault()?.Value?.ToString();

            switch (command.CommandName)
            {
                case "define":
                    if (!string.IsNullOrEmpty(word))
                    {
                        await HandleDictionaryCommandAsync(command, word, _dictionaryService.GetDefinitionAsync, _dictionaryService.IsWordOffensiveAsync);
                    }
                    break;

                case "variants":
                    if (!string.IsNullOrEmpty(word))
                    {
                        await HandleDictionaryCommandAsync(command, word, _dictionaryService.GetVariantsAsync, _dictionaryService.IsWordOffensiveAsync);
                    }
                    break;

                case "etymology":
                    if (!string.IsNullOrEmpty(word))
                    {
                        await HandleDictionaryCommandAsync(command, word, _dictionaryService.GetEtymologyAsync, _dictionaryService.IsWordOffensiveAsync);
                    }
                    break;
                
                case "synonyms":
                    if (!string.IsNullOrEmpty(word))
                    {
                        await HandleDictionaryCommandAsync(command, word, _dictionaryService.GetSynonymsAsync, _dictionaryService.IsWordOffensiveAsync);
                    }
                    break;

                case "info":
                    await _instanceInfo.HandleBotInfoCommandAsync(command);
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