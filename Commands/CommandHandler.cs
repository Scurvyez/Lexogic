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
            switch (command.CommandName)
            {
                case "define":
                {
                    string word = command.Data.Options.First().Value.ToString() ?? string.Empty;
                    string definition = await _dictionaryService.GetDefinitionAsync(word);

                    if (string.IsNullOrEmpty(definition))
                    {
                        await command.RespondAsync($"No definition(s) found for **{word}**.");
                    }
                    else
                    {
                        await command.RespondAsync($"**{word}**:{definition}");
                    }
                    break;
                }
                case "variants":
                {
                    string word = command.Data.Options.First().Value.ToString() ?? string.Empty;
                    string variants = await _dictionaryService.GetVariantsAsync(word);

                    if (string.IsNullOrEmpty(variants))
                    {
                        await command.RespondAsync($"No variant spelling(s) found for **{word}**.");
                    }
                    else
                    {
                        await command.RespondAsync($"**{word}**{variants}");
                    }
                    break;
                }
                case "etymology":
                {
                    string word = command.Data.Options.First().Value.ToString() ?? string.Empty;
                    string etymology = await _dictionaryService.GetEtymologyAsync(word);

                    if (string.IsNullOrEmpty(etymology))
                    {
                        await command.RespondAsync($"No known etymologies found for **{word}**.");
                    }
                    else
                    {
                        await command.RespondAsync($"**{word}**:{etymology}");
                    }
                    break;
                }
            }
        }
    }
}