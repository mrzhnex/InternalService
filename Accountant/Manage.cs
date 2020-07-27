using Discord;
using Discord.WebSocket;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InternalService.Accountant
{
    public static class Manage
    {
        public static void UpdateFullRpWhitelist(string message)
        {
            try
            {
                File.WriteAllText(Info.FullRpWhitelistFullFileName, message, System.Text.Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
                SendMessage(Info.ServerId, Info.ManageChannelId, "Ошибка при обновлении данных в файле: " + ex.Message);
            }
        }
        public static void UpdatePluginIdentify(string message)
        {
            try
            {
                File.WriteAllText(Info.PluginIdentifyFullFileName, message, System.Text.Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
                SendMessage(Info.ServerId, Info.ManageChannelId, "Ошибка при обновлении данных в файле: " + ex.Message);
            }
        }
        public static void UpdatePluginIdentifyRandom(string message)
        {
            try
            {
                File.WriteAllText(Info.PluginIdentifyRandomFullFileName, message, System.Text.Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
                SendMessage(Info.ServerId, Info.ManageChannelId, "Ошибка при обновлении данных в файле: " + ex.Message);
            }
        }
        public static bool CheckUserIsAccountant(ulong discordId)
        {
            if (Main.Manage.DiscordSocketClient.GetGuild(Info.ServerId).GetUser(discordId) != null)
            {
                if (Main.Manage.DiscordSocketClient.GetGuild(Info.ServerId).GetUser(discordId).Roles.Where(x => x.Id == Info.AccountantRoleId).FirstOrDefault() != default)
                    return true;
            }
            return false;
        }
        public static void SendMessage(ulong serverId, ulong channelId, string message)
        {
            Main.Manage.DiscordSocketClient.GetGuild(serverId).GetTextChannel(channelId).SendMessageAsync(message);
        }
        public static async Task OnMessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            ulong userdiscordid = arg2.Author.Id;
            if (!CheckUserIsAccountant(userdiscordid))
                return;
            string message = string.Empty;
            switch (arg2.Channel.Id)
            {
                case Info.FullRpWhitelistChannelId:
                    for (int i = arg2.Channel.GetMessagesAsync(100).Flatten().CountAsync().Result - 1; i > -1; i--)
                    {
                        if (i == 0)
                            message += arg2.Channel.GetMessagesAsync(100).Flatten().ToArrayAsync().Result[i].Content;
                        else
                            message = message + arg2.Channel.GetMessagesAsync(100).Flatten().ToArrayAsync().Result[i].Content + "\n";
                    }
                    UpdateFullRpWhitelist(message);
                    await Main.Manage.DiscordSocketClient.GetGuild(Info.ServerId).GetTextChannel(Info.ManageChannelId).SendMessageAsync("Обновлен список " + Main.Manage.DiscordSocketClient.GetGuild(Info.ServerId).GetTextChannel(Info.FullRpWhitelistChannelId).Mention);
                    break;
                case Info.PluginIdentifyChannelId:
                    for (int i = arg2.Channel.GetMessagesAsync(100).Flatten().CountAsync().Result - 1; i > -1; i--)
                    {
                        if (i == 0)
                            message += arg2.Channel.GetMessagesAsync(100).Flatten().ToArrayAsync().Result[i].Content;
                        else
                            message = message + arg2.Channel.GetMessagesAsync(100).Flatten().ToArrayAsync().Result[i].Content + "\n";
                    }
                    UpdatePluginIdentify(message);
                    await Main.Manage.DiscordSocketClient.GetGuild(Info.ServerId).GetTextChannel(Info.ManageChannelId).SendMessageAsync("Обновлен список " + Main.Manage.DiscordSocketClient.GetGuild(Info.ServerId).GetTextChannel(Info.PluginIdentifyChannelId).Mention);
                    break;
                case Info.PluginIdentifyRandomChannelId:
                    for (int i = arg2.Channel.GetMessagesAsync(100).Flatten().CountAsync().Result - 1; i > -1; i--)
                    {
                        if (i == 0)
                            message += arg2.Channel.GetMessagesAsync(100).Flatten().ToArrayAsync().Result[i].Content;
                        else
                            message = message + arg2.Channel.GetMessagesAsync(100).Flatten().ToArrayAsync().Result[i].Content + "\n";
                    }
                    UpdatePluginIdentifyRandom(message);
                    await Main.Manage.DiscordSocketClient.GetGuild(Info.ServerId).GetTextChannel(Info.ManageChannelId).SendMessageAsync("Обновлен список " + Main.Manage.DiscordSocketClient.GetGuild(Info.ServerId).GetTextChannel(Info.PluginIdentifyRandomChannelId).Mention);
                    break;
            }
            return;
        }
    }
}