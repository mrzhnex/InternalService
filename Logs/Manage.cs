using Discord;
using Discord.WebSocket;
using InternalService.Main;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InternalService.Logs
{
    public static class Manage
    {
        public static readonly Dictionary<ulong, LogsDialogue> DiscordUsersLogsDialogue = new Dictionary<ulong, LogsDialogue>();

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

        public static bool CheckUserHaveAccessToLogs(ulong discordId)
        {
            if (Main.Manage.DiscordSocketClient.GetGuild(Info.MainServerId).GetUser(discordId) != null)
            {
                foreach (ulong id in AccessLogsRolesId)
                {
                    if (Main.Manage.DiscordSocketClient.GetGuild(Info.MainServerId).GetUser(discordId).Roles.Where(x => x.Id == id).FirstOrDefault() != default)
                        return true;
                }
            }
            return false;
        }

        public static async Task OnMessageReceived(SocketMessage arg)
        {
            if (arg.Content is null || arg.Author.IsBot || arg.Channel.ToString()[0] != '@')
            {
                return;
            }
            ulong userdiscordid = arg.Author.Id;

            if (CheckUserHaveAccessToLogs(userdiscordid))
            {
                if (DiscordUsersLogsDialogue.ContainsKey(userdiscordid))
                {
                    if (arg.Content.ToLower() == (Internal.Info.Prefix + "exit"))
                    {
                        if (arg.Channel.GetMessageAsync(DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage).Result != null)
                            await arg.Channel.DeleteMessageAsync(DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage);
                        DiscordUsersLogsDialogue.Remove(userdiscordid);
                        await arg.Author.SendFileAsync(Path.Combine("/etc/scpsl/Plugin/", Info.LogsExitFileName), "До связи...");
                        return;
                    }
                    switch (DiscordUsersLogsDialogue[userdiscordid].LogsDialogueStage)
                    {
                        case LogsDialogueStage.Server:
                            if (int.TryParse(arg.Content, out int id) && Servers.Where(x => x.Id == id).FirstOrDefault() != default)
                            {
                                DiscordUsersLogsDialogue[userdiscordid].LogsDialogueStage = LogsDialogueStage.Day;
                                DiscordUsersLogsDialogue[userdiscordid].Server = Servers.Where(x => x.Id == id).FirstOrDefault();
                                if (arg.Channel.GetMessageAsync(DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage).Result != null)
                                    await arg.Channel.DeleteMessageAsync(DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage);
                                DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage = arg.Author.SendMessageAsync(Internal.Info.QuestionsLogs[DiscordUsersLogsDialogue[userdiscordid].LogsDialogueStage] + GetMessageByDate(Servers.Where(x => x.Id == id).FirstOrDefault())).Result.Id;
                            }
                            else
                            {
                                await arg.Channel.DeleteMessageAsync(DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage);
                                DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage = arg.Author.SendMessageAsync(Internal.Info.QuestionsLogs[DiscordUsersLogsDialogue[userdiscordid].LogsDialogueStage] + GetMessageByServer(Servers)).Result.Id;
                            }
                            break;
                        case LogsDialogueStage.Day:
                            if (int.TryParse(arg.Content, out int id_2) && DiscordUsersLogsDialogue[userdiscordid].Server.Logs.Where(x => x.Id == id_2).FirstOrDefault() != default)
                            {
                                DiscordUsersLogsDialogue[userdiscordid].LogsDialogueStage = LogsDialogueStage.Current;
                                DiscordUsersLogsDialogue[userdiscordid].Date = DiscordUsersLogsDialogue[userdiscordid].Server.Logs.Where(x => x.Id == id_2).FirstOrDefault().Date;
                                await arg.Channel.DeleteMessageAsync(DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage);
                                DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage = arg.Author.SendMessageAsync(Internal.Info.QuestionsLogs[DiscordUsersLogsDialogue[userdiscordid].LogsDialogueStage] + GetMessageByTime(GetLogsFromCurrentDate(DiscordUsersLogsDialogue[userdiscordid].Server, DiscordUsersLogsDialogue[userdiscordid].Date))).Result.Id;
                            }
                            else
                            {
                                await arg.Channel.DeleteMessageAsync(DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage);
                                DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage = arg.Author.SendMessageAsync(Internal.Info.QuestionsLogs[DiscordUsersLogsDialogue[userdiscordid].LogsDialogueStage] + GetMessageByDate(DiscordUsersLogsDialogue[userdiscordid].Server)).Result.Id;
                            }
                            break;
                        case LogsDialogueStage.Current:
                            if (int.TryParse(arg.Content, out int id_3) && GetLogsFromCurrentDate(DiscordUsersLogsDialogue[userdiscordid].Server, DiscordUsersLogsDialogue[userdiscordid].Date).Where(x => x.Id == id_3).FirstOrDefault() != default)
                            {
                                DiscordUsersLogsDialogue[userdiscordid].Log = GetLogsFromCurrentDate(DiscordUsersLogsDialogue[userdiscordid].Server, DiscordUsersLogsDialogue[userdiscordid].Date).Where(x => x.Id == id_3).FirstOrDefault();
                                await arg.Channel.DeleteMessageAsync(DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage);
                                await arg.Author.SendFileAsync(DiscordUsersLogsDialogue[userdiscordid].Log.FullFileName);
                                DiscordUsersLogsDialogue.Remove(userdiscordid);
                            }
                            else
                            {
                                await arg.Channel.DeleteMessageAsync(DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage);
                                DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage = arg.Author.SendMessageAsync(Internal.Info.QuestionsLogs[DiscordUsersLogsDialogue[userdiscordid].LogsDialogueStage] + GetMessageByTime(GetLogsFromCurrentDate(DiscordUsersLogsDialogue[userdiscordid].Server, DiscordUsersLogsDialogue[userdiscordid].Date))).Result.Id;
                            }
                            break;
                    }
                }
                else
                {
                    if (arg.Content.ToLower() == (Internal.Info.Prefix + "logs get").ToLower())
                    {
                        DiscordUsersLogsDialogue[userdiscordid] = new LogsDialogue()
                        {
                            LogsDialogueStage = LogsDialogueStage.Server,
                            LastSessionMessage = 1
                        };
                        DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage = arg.Author.SendMessageAsync(Internal.Info.QuestionsLogs[DiscordUsersLogsDialogue[userdiscordid].LogsDialogueStage] + GetMessageByServer(Servers)).Result.Id;
                    }
                }
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

        public static void UpdateServers()
        {
            int count;
            while (Info.Active)
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

        #region helper
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