using Discord;

namespace Lexogic
{
    public static class CommandBuilder
    {
        public static SlashCommandBuilder BuildDefineCommand()
        {
            return new SlashCommandBuilder()
                .WithName("define")
                .WithDescription("Look up the definition(s) of a word.")
                .AddOption("word", ApplicationCommandOptionType.String, 
                    "The word to define", isRequired: true);
        }

        public static SlashCommandBuilder BuildVariantsCommand()
        {
            return new SlashCommandBuilder()
                .WithName("variants")
                .WithDescription("Look up the variant spelling(s) of a word.")
                .AddOption("word", ApplicationCommandOptionType.String, 
                    "The word to get variant(s) for", isRequired: true);
        }
        
        public static SlashCommandBuilder BuildEtymologyCommand()
        {
            return new SlashCommandBuilder()
                .WithName("etymology")
                .WithDescription("Look up the etymology of a word.")
                .AddOption("word", ApplicationCommandOptionType.String, 
                    "The word to get any known etymologies for", isRequired: true);
        }
    }
}