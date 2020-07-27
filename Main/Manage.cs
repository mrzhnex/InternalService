using System.Linq;
using System.IO;
using System.Threading;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using System;
using InternalService.Internal;

namespace InternalService.Main
{
    public static class Manage
    {
        public static DiscordSocketClient DiscordSocketClient { get; set; }
        public static async Task Log(LogType logType, string message, ulong serverId = Info.MainServerId, ulong channelId = Info.LogChannelId)
        {
            message = "[" + logType.ToString() + "]: " + message;
            Console.WriteLine(message);
            await DiscordSocketClient.GetGuild(serverId).GetTextChannel(channelId).SendMessageAsync(message);
        }
        public static void Main(string [] args)
        {
            if (args.Length > 0 && bool.TryParse(args[0], out bool isHost))
            {
                Info.IsHost = isHost;
            }
            Console.WriteLine("{0} - {1}", nameof(Info.Version), Info.Version);
            Console.WriteLine("{0} - {1}", nameof(Info.IsHost), Info.IsHost.ToString());
            SetBotToken();
            SetConnetionString();
            Console.WriteLine("{0} - {1} ", nameof(Info.BotToken), Info.BotToken);
            Console.WriteLine("{0} - {1}", nameof(Administrative.Info.ConnectionString), Administrative.Info.ConnectionString);
            if (Info.IsHost)
            {
                SaveLoad.LoadReporters();
                SaveLoad.LoadReportId();
                SaveLoad.SaveReporters();
                SaveLoad.SaveReportId();
            }
            Thread thread = new Thread(RunBotAsync().GetAwaiter().GetResult);
            thread.Start();
            if (Info.IsHost)
            {
                Thread checkForUpdate = new Thread(FullRP.Manage.LoopCheckForUpdate);
                checkForUpdate.Start();
                Thread UpdateServeers = new Thread(Logs.Manage.UpdateServers);
                UpdateServeers.Start();
            }
        }
        public static Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
        public static async Task RunBotAsync()
        {
            DiscordSocketClient = new DiscordSocketClient();
            DiscordSocketClient.Log += Log;
            if (Info.IsHost)
            {
                DiscordSocketClient.MessageReceived += OnMessageReceived;
                DiscordSocketClient.MessageReceived += Logs.Manage.OnMessageReceived;
                DiscordSocketClient.MessageReceived += Internal.Manage.OnMessageReceived;
                DiscordSocketClient.MessageUpdated += Accountant.Manage.OnMessageUpdated;
            }
            DiscordSocketClient.MessageReceived += Administrative.Manage.OnMessageReceived;
            DiscordSocketClient.UserBanned += Administrative.Manage.OnUserBanned;
            DiscordSocketClient.UserLeft += Administrative.Manage.OnUserLeft;
            DiscordSocketClient.RoleDeleted += Administrative.Manage.OnRoleDeleted;
            DiscordSocketClient.RoleUpdated += Administrative.Manage.OnRoleUpdated;
            DiscordSocketClient.GuildMemberUpdated += Administrative.Manage.OnGuildMemberUpdated;
            DiscordSocketClient.Ready += Administrative.Manage.OnReady;
            await DiscordSocketClient.LoginAsync(TokenType.Bot, Info.BotToken);
            await DiscordSocketClient.StartAsync();
            await Task.Delay(-1);
        }
        public static async Task OnMessageReceived(SocketMessage arg)
        {
            if (arg.Content == null || arg.Author.IsBot || arg.Channel.Id != 689181158730104973 || arg.Content.ToLower() != "!emptyroles")
            {
                return;
            }
            string message = "Пустые роли:\n";
            int count = 0;
            foreach (SocketRole role in DiscordSocketClient.GetGuild(Info.MainServerId).Roles)
            {
                if (role.Members.Count() == 0)
                {
                    count++;
                    message = message + "\n" + count + ") " + role.Mention;
                }
            }
            await DiscordSocketClient.GetGuild(Info.MainServerId).GetTextChannel(689181158730104973).SendMessageAsync(message + "\n");
        }
        public static void SetBotToken()
        {
            if (Info.IsHost)
                Info.BotToken = File.ReadAllText(Path.Combine("/etc/scpsl/Administrative/", "DiscordBotToken.txt"));
            else
                Info.BotToken = File.ReadAllText(Path.Combine("E:/Info/", "DiscordBotToken.txt"));
        }
        public static void SetConnetionString()
        {
            if (Info.IsHost)
                Administrative.Info.ConnectionString = File.ReadAllText(Path.Combine("/etc/scpsl/Administrative/", "ConnectionString.txt"));
            else
                Administrative.Info.ConnectionString = File.ReadAllText(Path.Combine("E:/Info/", "ConnectionString.txt"));
        }
    }
    public enum LogType
    {
        Trace, Info, Debug, Warn, Error, Fatal
    }
}