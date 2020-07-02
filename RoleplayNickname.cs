﻿using System.IO;

namespace InternalService
{
    public static class RoleplayNickname
    {
        public const ulong ServerId = 707251593048752191;
        public const ulong FullRpWhitelistChannelId = 707275268871553075;
        public const ulong PluginIdentifyChannelId = 707275980095619112;
        public const ulong PluginIdentifyRandomChannelId = 707275766123200602;
        public const ulong AccountantRoleId = 707275136776142900;
        public const ulong ManageChannelId = 710245931533992046;

        public static readonly string FullRpWhitelistFullFileName = Path.Combine("/etc/scpsl/config_common/SteamIDWhitelist.txt");
        public static readonly string PluginIdentifyFullFileName = Path.Combine("/etc/PluginData/FixedNames.txt");
        public static readonly string PluginIdentifyRandomFullFileName = Path.Combine("/etc/PluginData/RandomNames.txt");

        public static void UpdateFullRpWhitelist(string message)
        {
            try
            {
                File.WriteAllText(FullRpWhitelistFullFileName, message, System.Text.Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
                Program._client.GetGuild(ServerId).GetTextChannel(ManageChannelId).SendMessageAsync("Ошибка при обновлении данных в файле: " + ex.Message);
            }
        }

        public static void UpdatePluginIdentify(string message)
        {
            try
            {
                File.WriteAllText(PluginIdentifyFullFileName, message, System.Text.Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
                Program._client.GetGuild(ServerId).GetTextChannel(ManageChannelId).SendMessageAsync("Ошибка при обновлении данных в файле: " + ex.Message);
            }
        }

        public static void UpdatePluginIdentifyRandom(string message)
        {
            try
            {
                File.WriteAllText(PluginIdentifyRandomFullFileName, message, System.Text.Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
                Program._client.GetGuild(ServerId).GetTextChannel(ManageChannelId).SendMessageAsync("Ошибка при обновлении данных в файле: " + ex.Message);
            }
        }
    }
}