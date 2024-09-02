﻿using Discord;
using Discord.WebSocket;
using System.Text.Json;
using Discord.Net;

namespace Lexogic
{
    public class Program
    {
        private DiscordSocketClient _client;
        private CommandHandler _commandHandler;

        public static Task Main(string[] args) => new Program().MainAsync();

        private async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;

            DictionaryService dictionaryService = new (new HttpClient());
            _commandHandler = new CommandHandler(dictionaryService);

            string? token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

            if (string.IsNullOrEmpty(token))
            {
                PrefixedConsole.WriteLine("Error: Bot token is not set or is empty!");
                return;
            }

            PrefixedConsole.WriteLine($"Bot token retrieved: {token.Length} characters long.");
            PrefixedConsole.WriteLine($"Token snippet: {token.Substring(0, 3)}...{token.Substring(token.Length - 3)}");

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _client.SlashCommandExecuted += SlashCommandExecutedAsync;

            await Task.Delay(-1);
        }

        private static Task LogAsync(LogMessage log)
        {
            PrefixedConsole.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private async Task ReadyAsync()
        {
            try
            {
                await _client.Rest.CreateGlobalCommand(CommandBuilder.BuildDefineCommand().Build());
                await _client.Rest.CreateGlobalCommand(CommandBuilder.BuildVariantsCommand().Build());
                await _client.Rest.CreateGlobalCommand(CommandBuilder.BuildEtymologyCommand().Build());

                PrefixedConsole.WriteLine("Global slash commands /define, /variants, /etymology registered.");
            }
            catch (HttpException ex)
            {
                string json = JsonSerializer.Serialize(ex.Errors, new JsonSerializerOptions { WriteIndented = true });
                PrefixedConsole.WriteLine(json);
            }
        }

        private async Task SlashCommandExecutedAsync(SocketSlashCommand command)
        {
            await _commandHandler.HandleSlashCommandAsync(command);
        }
    }
}