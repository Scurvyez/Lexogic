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
                .AddOption("word", ApplicationCommandOptionType.String, "The word to define", isRequired: true);
        }

        public static SlashCommandBuilder BuildVarsCommand()
        {
            return new SlashCommandBuilder()
                .WithName("vars")
                .WithDescription("Look up the variant spelling(s) of a word.")
                .AddOption("word", ApplicationCommandOptionType.String, "The word to get variant(s) for", isRequired: true);
        }
    }
}