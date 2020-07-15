using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using InternalService.Logs;

namespace InternalService.Manage
{
    public static class Global
    {
        public static bool Active = true;

        public static List<Server> Servers = new List<Server>();

        public static readonly Dictionary<string, string> ServersPathAndName = new Dictionary<string, string>()
        {
            {"/home/scpslfullrp/.config/SCP Secret Laboratory/ServerLogs/7767/", "Full RP" },
            {"/home/scpsllightrp1/.config/SCP Secret Laboratory/ServerLogs/7777/", "Light RP #1" },
            {"/home/scpsllightrp2/.config/SCP Secret Laboratory/ServerLogs/7778/", "Light RP #2" },
            {"/home/scpsllightrp3/.config/SCP Secret Laboratory/ServerLogs/7779/", "Light RP #3" },
            {"/home/scpsllightrp4/.config/SCP Secret Laboratory/ServerLogs/7780/", "Light RP #4" },
            {"/home/scpslmediumrp1/.config/SCP Secret Laboratory/ServerLogs/7781/", "Medium RP #1" },
            {"/home/scpslmediumrp2/.config/SCP Secret Laboratory/ServerLogs/7782/", "Medium RP #2" },
            {"/home/scpsllightrpevent/.config/SCP Secret Laboratory/ServerLogs/7783/", "Light RP Event" },
            {"/home/scpslnonrp1/.config/SCP Secret Laboratory/ServerLogs/7784/", "Light Non-RP #1" },
            {"/home/scpslnonrp2/.config/SCP Secret Laboratory/ServerLogs/7785/", "Light Non-RP #2" },
            {"/home/scpsllightrplightsout/.config/SCP Secret Laboratory/ServerLogs/7786/", "Light RP Lights-out" },
        };

        public static readonly List<ulong> AccessLogsRolesId = new List<ulong>()
        {
            434261615467036673,
            500749870953660426,
            434261263283912714,
            447805014686302209,
            500750905000591371,
            500757073848434689,
            498563720935637002,
            435747850328997889
        };

        #region thread
        public static void UpdateServers()
        {
            int count;
            while (Active)
            {
                Servers = new List<Server>();
                count = 0;
                foreach (KeyValuePair<string, string> keyValuePair in ServersPathAndName)
                {
                    Servers.Add(new Server()
                    {
                        Id = count,
                        LogsPath = keyValuePair.Key,
                        Name = keyValuePair.Value,
                        Logs = UpdateLogs(keyValuePair.Key)
                    });
                    count++;
                }
                Thread.Sleep(60000);
            }
        }
        private static List<Log> UpdateLogs(string logsPath)
        {
            List<Log> logs = new List<Log>();
            int count = 0;
            FileInfo[] files = new DirectoryInfo(logsPath).GetFiles().OrderBy(x => x.LastWriteTime).ToArray();
            for (int i = 0; i < files.Length; i++)
            {
                logs.Add(new Log()
                {
                    Id = count,
                    Date = GetDateByLogFileName(files[i].Name),
                    Time = GetTimeByLogFileName(files[i].Name),
                    FullFileName = files[i].FullName
                });
                count++;
            }
            return logs;
        }
        #endregion

        public static string GetMessageByTime(List<Log> logs)
        {
            string message = "\n";
            foreach (Log log in logs)
            {
                message = message + log.Id + ") " + log.Time + "\n";
            }
            return message;
        }
        public static string GetMessageByDate(Server server)
        {
            string message = "\n";
            foreach (Log log in server.Logs)
            {
                if (!message.Contains(log.Date))
                    message = message + log.Id + ") " + log.Date + "\n";
            }
            return message;
        }
        public static string GetMessageByServer(List<Server> servers)
        {
            string message = "\n";
            foreach (Server server in servers)
            {
                message = message + server.Id + ") " + server.Name + "\n";
            }
            return message;
        }

        #region helper
        public static List<Log> GetLogsFromCurrentDate(Server server, string date)
        {
            return server.Logs.Where(x => x.Date == date).ToList();
        }
        private static string GetDateByLogFileName(string fileName)
        {
            return fileName.Substring(6, 10);
        }
        private static string GetTimeByLogFileName(string fileName)
        {
            return fileName.Substring(17, 8);
        }
        #endregion
    }
}