using Discord.Rest;
using Discord.WebSocket;

namespace Lexogic
{
    public class InstanceInfo
    {
        private readonly DiscordSocketClient _client;

        public InstanceInfo(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task HandleBotInfoCommandAsync(SocketSlashCommand command)
        {
            RestApplication? appInfo = await _client.GetApplicationInfoAsync();

            string botInfo = $"**`Bot Name:`** {appInfo.Name}\n" +
                             $"**`Bot ID:`** {appInfo.Id}\n" +
                             $"**`Created On:`** {appInfo.CreatedAt.UtcDateTime}\n" +
                             $"**`Bot Uptime:`** {GetUptime()}";

            await command.RespondAsync(botInfo);
        }

        private string GetUptime()
        {
            TimeSpan uptime = DateTime.UtcNow - _client.CurrentUser.CreatedAt.UtcDateTime;
            return $"{uptime.Days} days, {uptime.Hours} hours, {uptime.Minutes} minutes";
        }
    }
}