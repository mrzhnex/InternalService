using System.Collections.Generic;
using System.IO;

namespace InternalService.Manage
{
    public static class Info
    {
        public static readonly string Version = "1.A.5";

        public static string BotToken = "";
        public static readonly ulong ServerId = 678659777550745608;

        public static readonly ulong ReportChannelId = 678908085615591431;
        public static readonly ulong ManageChannelId = 678909048225005569;
        public static readonly ulong LogsChannelId = 678908863969361920;
        public static readonly ulong ReportFullChannelId = 678916367637938196;
        public static readonly ulong FullLogsChannelID = 678913763218292747;

        public static List<ulong> Reporters = new List<ulong>();

        public static readonly string ReportersFileName = "Reporters.xml";
        public static readonly string ReportIdFileName = "ReportId.xml";
        public static readonly string LogsExitFileName = "ToCommunication.jpg";

        private static readonly string DataFolderName = "Internal";
        private static readonly string HostDataPath = Path.Combine("/etc/scpsl/", DataFolderName);

        private static readonly string BotTokenFileName = "DiscordBotToken.txt";
        private static readonly string BotTokenFilePath = Path.Combine("/etc/scpsl/Administrative/", BotTokenFileName);

        public static string GetDataPath()
        {
            return HostDataPath;
        }

        public static string GetBotToken()
        {
            return File.ReadAllText(BotTokenFilePath);
        }

        public static void CreateData()
        {
            if (!Directory.Exists(GetDataPath()))
            {
                Directory.CreateDirectory(GetDataPath());
            }

            if (!File.Exists(Path.Combine(GetDataPath(), ReportersFileName)))
            {
                File.Create(Path.Combine(GetDataPath(), ReportersFileName));
            }

            if (!File.Exists(Path.Combine(GetDataPath(), ReportIdFileName)))
            {
                File.Create(Path.Combine(GetDataPath(), ReportIdFileName));
            }
        }
    }
}