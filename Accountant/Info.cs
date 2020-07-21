using System.IO;

namespace InternalService.Accountant
{
    public static class Info
    {
        public const ulong ServerId = 707251593048752191;
        public const ulong FullRpWhitelistChannelId = 707275268871553075;
        public const ulong PluginIdentifyChannelId = 707275980095619112;
        public const ulong PluginIdentifyRandomChannelId = 707275766123200602;
        public const ulong AccountantRoleId = 707275136776142900;
        public const ulong ManageChannelId = 710245931533992046;

        public static readonly string FullRpWhitelistFullFileName = Path.Combine("/etc/scpsl/Main/UserIDWhitelist.txt");
        public static readonly string PluginIdentifyFullFileName = Path.Combine("/etc/scpsl/Plugin/FixedNames.txt");
        public static readonly string PluginIdentifyRandomFullFileName = Path.Combine("/etc/scpsl/Plugin/RandomNames.txt");
    }
}