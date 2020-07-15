using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InternalService.Accountant;
using InternalService.Internal;
using InternalService.Manage;
using InternalService.Logs;

namespace InternalService
{
    public class Program
    {
        public static Program ProgramSession;
        public DiscordSocketClient DiscordSocketClient { get; set; }

        #region Main functions
        public async Task RunBotAsync()
        {
            DiscordSocketClient = new DiscordSocketClient();
            DiscordSocketClient.Log += Log;
            RegisterCommand();
            await DiscordSocketClient.LoginAsync(TokenType.Bot, Info.BotToken);
            await DiscordSocketClient.StartAsync();
            await Task.Delay(-1);
        }
        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
        public void RegisterCommand()
        {
            DiscordSocketClient.MessageReceived += HandleCommandAsync;
            DiscordSocketClient.MessageUpdated += HandleMessageUpdatedAsync;
        }
        public static void Main()
        {
            Console.WriteLine("Program version - " + Info.Version);
            Info.CreateData();
            SaveLoad.LoadReporters();
            SaveLoad.LoadReportId();
            SaveLoad.SaveReporters();
            SaveLoad.SaveReportId();
            Info.BotToken = Info.GetBotToken();
            ProgramSession = new Program();
            Thread thread = new Thread(ProgramSession.RunBotAsync().GetAwaiter().GetResult);
            thread.Start();
            Thread thread1 = new Thread(FullRP.PlayersWhoPlayedMethods.LoopCheckForUpdate);
            thread1.Start();
            Thread thread2 = new Thread(Global.UpdateServers);
            thread2.Start();
        }
        public async Task HandleCommandAsync(SocketMessage arg)
        {
            if (arg.Content is null || arg.Author.IsBot)
            {
                return;
            }
            if (arg.Channel.Id == 689181158730104973 && arg.Content.ToLower() == "!emptyroles")
            {
                WriteEmptyRoles();
            }

            if (arg.Channel.Id == Info.ManageChannelId)
            {
                if (arg.Content.ToLower().Contains((KeyWord.Prefix + KeyWord.AddReporter).ToLower()) && arg.Content.ToLower().IndexOf((KeyWord.Prefix + KeyWord.AddReporter).ToLower()) == 0)
                {
                    string[] messageMass = arg.Content.Split(" ");
                    if (messageMass.Length > 1)
                    {
                        if (ulong.TryParse(messageMass[1], out ulong reporterId))
                        {
                            if (!Info.Reporters.Contains(reporterId))
                            {
                                Info.Reporters.Add(reporterId);
                                SaveLoad.SaveReporters();
                                await arg.Channel.SendMessageAsync(KeyWord.SuccessAdd);
                            }
                            else
                            {
                                await arg.Channel.SendMessageAsync("Пользователь уже в списке!");
                            }
                        }
                        else
                        {
                            await arg.Channel.SendMessageAsync("Неверный идентификатор!");
                        }
                    }
                    else
                    {
                        await arg.Channel.SendMessageAsync("Требуется идентификатор!");
                    }
                }
                else if (arg.Content.ToLower().Contains((KeyWord.Prefix + KeyWord.RemoveReporter).ToLower()) && arg.Content.ToLower().IndexOf((KeyWord.Prefix + KeyWord.RemoveReporter).ToLower()) == 0)
                {
                    string[] messageMass = arg.Content.Split(" ");
                    if (messageMass.Length > 1)
                    {
                        if (ulong.TryParse(messageMass[1], out ulong reporterId))
                        {
                            if (Info.Reporters.Contains(reporterId))
                            {
                                Info.Reporters.Remove(reporterId);
                                SaveLoad.SaveReporters();
                                await arg.Channel.SendMessageAsync(KeyWord.SuccessRemove);
                            }
                            else
                            {
                                await arg.Channel.SendMessageAsync("Пользователя нет в списке!");
                            }
                        }
                        else
                        {
                            await arg.Channel.SendMessageAsync("Неверный идентификатор!");
                        }
                    }
                    else
                    {
                        await arg.Channel.SendMessageAsync("Требуется идентификатор!");
                    }
                }
                else if (arg.Content.ToLower().Contains((KeyWord.Prefix + KeyWord.ListReporter).ToLower()) && arg.Content.ToLower().IndexOf((KeyWord.Prefix + KeyWord.ListReporter).ToLower()) == 0)
                {
                    string answer = string.Empty;
                    foreach (ulong id in Info.Reporters)
                    {
                        if (DiscordSocketClient.GetUser(id) != null)
                        {
                            answer = answer + DiscordSocketClient.GetUser(id).Mention + "\n";
                        }
                        else
                        {
                            answer = answer + id + "\n";
                        }
                    }
                    await arg.Channel.SendMessageAsync("Список пользователей:\n" + answer);
                }
                else
                {
                    await arg.Channel.SendMessageAsync("Команда не найдена!");
                }
            }
            if (arg.Channel.ToString()[0] != '@')
            {
                return;
            }
            ulong userdiscordid = arg.Author.Id;

            if (CheckUserHaveAccessToLogs(userdiscordid))
            {
                if (Session.DiscordUsersLogsDialogue.ContainsKey(userdiscordid))
                {
                    if (arg.Content.ToLower() == (KeyWord.Prefix + "exit"))
                    {
                        if (arg.Channel.GetMessageAsync(Session.DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage).Result != null)
                            await arg.Channel.DeleteMessageAsync(Session.DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage);
                        Session.DiscordUsersLogsDialogue.Remove(userdiscordid);
                        await arg.Author.SendFileAsync(Path.Combine("/etc/scpsl/Plugin/", Info.LogsExitFileName), "До связи...");
                        return;
                    }
                    switch (Session.DiscordUsersLogsDialogue[userdiscordid].LogsDialogueStage)
                    {
                        case LogsDialogueStage.Server:
                            if (int.TryParse(arg.Content, out int id) && Global.Servers.Where(x => x.Id == id).FirstOrDefault() != default)
                            {
                                Session.DiscordUsersLogsDialogue[userdiscordid].LogsDialogueStage = LogsDialogueStage.Day;
                                Session.DiscordUsersLogsDialogue[userdiscordid].Server = Global.Servers.Where(x => x.Id == id).FirstOrDefault();
                                if (arg.Channel.GetMessageAsync(Session.DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage).Result != null)
                                    await arg.Channel.DeleteMessageAsync(Session.DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage);
                                Session.DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage = arg.Author.SendMessageAsync(KeyWord.QuestionsLogs[Session.DiscordUsersLogsDialogue[userdiscordid].LogsDialogueStage] + Global.GetMessageByDate(Global.Servers.Where(x => x.Id == id).FirstOrDefault())).Result.Id;
                            }
                            else
                            {
                                await arg.Channel.DeleteMessageAsync(Session.DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage);
                                Session.DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage = arg.Author.SendMessageAsync(KeyWord.QuestionsLogs[Session.DiscordUsersLogsDialogue[userdiscordid].LogsDialogueStage] + Global.GetMessageByServer(Global.Servers)).Result.Id;
                            }
                            break;
                        case LogsDialogueStage.Day:
                            if (int.TryParse(arg.Content, out int id_2) && Session.DiscordUsersLogsDialogue[userdiscordid].Server.Logs.Where(x => x.Id == id_2).FirstOrDefault() != default)
                            {
                                Session.DiscordUsersLogsDialogue[userdiscordid].LogsDialogueStage = LogsDialogueStage.Current;
                                Session.DiscordUsersLogsDialogue[userdiscordid].Date = Session.DiscordUsersLogsDialogue[userdiscordid].Server.Logs.Where(x => x.Id == id_2).FirstOrDefault().Date;
                                await arg.Channel.DeleteMessageAsync(Session.DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage);
                                Session.DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage = arg.Author.SendMessageAsync(KeyWord.QuestionsLogs[Session.DiscordUsersLogsDialogue[userdiscordid].LogsDialogueStage] + Global.GetMessageByTime(Global.GetLogsFromCurrentDate(Session.DiscordUsersLogsDialogue[userdiscordid].Server, Session.DiscordUsersLogsDialogue[userdiscordid].Date))).Result.Id;
                            }
                            else
                            {
                                await arg.Channel.DeleteMessageAsync(Session.DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage);
                                Session.DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage = arg.Author.SendMessageAsync(KeyWord.QuestionsLogs[Session.DiscordUsersLogsDialogue[userdiscordid].LogsDialogueStage] + Global.GetMessageByDate(Session.DiscordUsersLogsDialogue[userdiscordid].Server)).Result.Id;
                            }
                            break;
                        case LogsDialogueStage.Current:
                            if (int.TryParse(arg.Content, out int id_3) && Global.GetLogsFromCurrentDate(Session.DiscordUsersLogsDialogue[userdiscordid].Server, Session.DiscordUsersLogsDialogue[userdiscordid].Date).Where(x => x.Id == id_3).FirstOrDefault() != default)
                            {
                                Session.DiscordUsersLogsDialogue[userdiscordid].Log = Global.GetLogsFromCurrentDate(Session.DiscordUsersLogsDialogue[userdiscordid].Server, Session.DiscordUsersLogsDialogue[userdiscordid].Date).Where(x => x.Id == id_3).FirstOrDefault();
                                await arg.Channel.DeleteMessageAsync(Session.DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage);
                                await arg.Author.SendFileAsync(Session.DiscordUsersLogsDialogue[userdiscordid].Log.FullFileName);
                                Session.DiscordUsersLogsDialogue.Remove(userdiscordid);
                            }
                            else
                            {
                                await arg.Channel.DeleteMessageAsync(Session.DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage);
                                Session.DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage = arg.Author.SendMessageAsync(KeyWord.QuestionsLogs[Session.DiscordUsersLogsDialogue[userdiscordid].LogsDialogueStage] + Global.GetMessageByTime(Global.GetLogsFromCurrentDate(Session.DiscordUsersLogsDialogue[userdiscordid].Server, Session.DiscordUsersLogsDialogue[userdiscordid].Date))).Result.Id;
                            }
                            break;
                    }
                }
                else
                {
                    if (arg.Content.ToLower() == (KeyWord.Prefix + "logs get").ToLower())
                    {
                        Session.DiscordUsersLogsDialogue[userdiscordid] = new LogsDialogue()
                        {
                            LogsDialogueStage = LogsDialogueStage.Server,
                            LastSessionMessage = 1
                        };
                        Session.DiscordUsersLogsDialogue[userdiscordid].LastSessionMessage = arg.Author.SendMessageAsync(KeyWord.QuestionsLogs[Session.DiscordUsersLogsDialogue[userdiscordid].LogsDialogueStage] + Global.GetMessageByServer(Global.Servers)).Result.Id;
                    }
                }
            }

            if (Info.Reporters.Contains(userdiscordid))
            {
                if (Claim.DiscordUsersReportDialogue.ContainsKey(userdiscordid))
                {
                    switch (Claim.DiscordUsersReportDialogue[userdiscordid].ReportStage)
                    {
                        case ReportStage.SuspectQuestion:
                            Claim.DiscordUsersReportDialogue[userdiscordid].Suspect = arg.Content;
                            Claim.DiscordUsersReportDialogue[userdiscordid].ReportStage = ReportStage.ReasonQuestion;
                            await arg.Channel.SendMessageAsync(KeyWord.QuestionsReport[Claim.DiscordUsersReportDialogue[userdiscordid].ReportStage]);
                            break;
                        case ReportStage.ReasonQuestion:
                            Claim.DiscordUsersReportDialogue[userdiscordid].Reason = arg.Content;
                            Claim.DiscordUsersReportDialogue[userdiscordid].ReportStage = ReportStage.ProofQuestion;
                            await arg.Channel.SendMessageAsync(KeyWord.QuestionsReport[Claim.DiscordUsersReportDialogue[userdiscordid].ReportStage]);
                            break;
                        case ReportStage.ProofQuestion:

                            if (arg.Content.ToLower() == (KeyWord.Prefix + KeyWord.ProofKeyWordEnd).ToLower())
                            {
                                Claim.DiscordUsersReportDialogue[userdiscordid].ReportStage = ReportStage.ConfirmSendQuestion;
                                await arg.Channel.SendMessageAsync(Claim.GetReportMessagePreView(Claim.DiscordUsersReportDialogue[userdiscordid]) + "\n" + KeyWord.ConfirmSendQuestion);
                            }
                            else
                            {
                                Claim.DiscordUsersReportDialogue[userdiscordid].Proof = Claim.DiscordUsersReportDialogue[userdiscordid].Proof + arg.Content + "\n";
                                foreach (Attachment attachment in arg.Attachments.ToList())
                                {
                                    Claim.DiscordUsersReportDialogue[userdiscordid].Proof = Claim.DiscordUsersReportDialogue[userdiscordid].Proof + attachment.ProxyUrl + "\n";
                                }
                            }
                            break;
                        case ReportStage.ConfirmSendQuestion:
                            if (arg.Content.ToLower() == KeyWord.ConfirmSendAnswerYes.ToLower() || arg.Content.ToLower() == "1")
                            {
                                string[] result = Claim.GetReportMessage(Claim.DiscordUsersReportDialogue[userdiscordid]);
                                await (DiscordSocketClient.GetGuild(Info.ServerId).GetChannel(Info.ReportChannelId) as IMessageChannel).SendMessageAsync(result[0]);
                                await (DiscordSocketClient.GetGuild(Info.ServerId).GetChannel(Info.ReportFullChannelId) as IMessageChannel).SendMessageAsync(Claim.GetFullReportMessage(Claim.DiscordUsersReportDialogue[userdiscordid], arg.Author.Id, ulong.Parse(result[1])));
                                await arg.Channel.SendMessageAsync("Ваша жалоба успешно отправлена.");
                                Claim.DiscordUsersReportDialogue.Remove(userdiscordid);
                            }
                            else if (arg.Content.ToLower() == KeyWord.ConfirmSendAnswerNo.ToLower() || arg.Content.ToLower() == "2")
                            {
                                string[] result = Claim.GetReportMessage(Claim.DiscordUsersReportDialogue[userdiscordid]);
                                await (DiscordSocketClient.GetGuild(Info.ServerId).GetChannel(Info.LogsChannelId) as IMessageChannel).SendMessageAsync(KeyWord.HiddenMessage + Claim.GetFullReportMessage(Claim.DiscordUsersReportDialogue[userdiscordid], arg.Author.Id, ulong.Parse(result[1])));
                                await arg.Channel.SendMessageAsync("Ваша жалоба не отправлена.");
                                Claim.DiscordUsersReportDialogue.Remove(userdiscordid);
                            }
                            else
                            {
                                await arg.Channel.SendMessageAsync(KeyWord.ConfirmSendQuestion);
                            }
                            break;
                        default:
                            Claim.DiscordUsersReportDialogue.Remove(userdiscordid);
                            return;
                    }
                }
                else
                {
                    if (arg.Content.ToLower() == (KeyWord.Prefix + KeyWord.Report).ToLower())
                    {
                        Claim.DiscordUsersReportDialogue[userdiscordid] = new Report()
                        {
                            ReportStage = ReportStage.SuspectQuestion,
                            Sender = arg.Author.Mention
                        };
                        await arg.Author.SendMessageAsync(KeyWord.QuestionsReport[Claim.DiscordUsersReportDialogue[userdiscordid].ReportStage]);
                    }
                    else
                    {
                        return;
                    }
                }
            }



        }
        public void SendMessage(ulong serverId, ulong channelId, string message)
        {
            DiscordSocketClient.GetGuild(serverId).GetTextChannel(channelId).SendMessageAsync(message);
        }
        #endregion

        #region Accountant
        public bool CheckUserIsAccountant(ulong discordId)
        {
            if (DiscordSocketClient.GetGuild(RoleplayNickname.ServerId).GetUser(discordId) != null)
            {
                if (DiscordSocketClient.GetGuild(RoleplayNickname.ServerId).GetUser(discordId).Roles.Where(x => x.Id == RoleplayNickname.AccountantRoleId).FirstOrDefault() != default)
                    return true;
            }
            return false;
        }
        private async Task HandleMessageUpdatedAsync(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            ulong userdiscordid = arg2.Author.Id;
            if (!CheckUserIsAccountant(userdiscordid))
                return;
            string message = string.Empty;
            switch (arg2.Channel.Id)
            {
                case RoleplayNickname.FullRpWhitelistChannelId:
                    for (int i = arg2.Channel.GetMessagesAsync(100).Flatten().CountAsync().Result - 1; i > -1; i--)
                    {
                        if (i == 0)
                            message += arg2.Channel.GetMessagesAsync(100).Flatten().ToArrayAsync().Result[i].Content;
                        else
                            message = message + arg2.Channel.GetMessagesAsync(100).Flatten().ToArrayAsync().Result[i].Content + "\n";
                    }
                    RoleplayNickname.UpdateFullRpWhitelist(message);
                    await DiscordSocketClient.GetGuild(RoleplayNickname.ServerId).GetTextChannel(RoleplayNickname.ManageChannelId).SendMessageAsync("Обновлен список " + DiscordSocketClient.GetGuild(RoleplayNickname.ServerId).GetTextChannel(RoleplayNickname.FullRpWhitelistChannelId).Mention);
                    break;
                case RoleplayNickname.PluginIdentifyChannelId:
                    for (int i = arg2.Channel.GetMessagesAsync(100).Flatten().CountAsync().Result - 1; i > -1; i--)
                    {
                        if (i == 0)
                            message += arg2.Channel.GetMessagesAsync(100).Flatten().ToArrayAsync().Result[i].Content;
                        else
                            message = message + arg2.Channel.GetMessagesAsync(100).Flatten().ToArrayAsync().Result[i].Content + "\n";
                    }
                    RoleplayNickname.UpdatePluginIdentify(message);
                    await DiscordSocketClient.GetGuild(RoleplayNickname.ServerId).GetTextChannel(RoleplayNickname.ManageChannelId).SendMessageAsync("Обновлен список " + DiscordSocketClient.GetGuild(RoleplayNickname.ServerId).GetTextChannel(RoleplayNickname.PluginIdentifyChannelId).Mention);
                    break;
                case RoleplayNickname.PluginIdentifyRandomChannelId:
                    for (int i = arg2.Channel.GetMessagesAsync(100).Flatten().CountAsync().Result - 1; i > -1; i--)
                    {
                        if (i == 0)
                            message += arg2.Channel.GetMessagesAsync(100).Flatten().ToArrayAsync().Result[i].Content;
                        else
                            message = message + arg2.Channel.GetMessagesAsync(100).Flatten().ToArrayAsync().Result[i].Content + "\n";
                    }
                    RoleplayNickname.UpdatePluginIdentifyRandom(message);
                    await DiscordSocketClient.GetGuild(RoleplayNickname.ServerId).GetTextChannel(RoleplayNickname.ManageChannelId).SendMessageAsync("Обновлен список " + DiscordSocketClient.GetGuild(RoleplayNickname.ServerId).GetTextChannel(RoleplayNickname.PluginIdentifyRandomChannelId).Mention);
                    break;
            }
            return;
        }

        #endregion

        #region Logs
        public bool CheckUserHaveAccessToLogs(ulong discordId)
        {
            if (DiscordSocketClient.GetGuild(351766372264574976).GetUser(discordId) != null)
            {
                foreach (ulong id in Global.AccessLogsRolesId)
                {
                    if (DiscordSocketClient.GetGuild(351766372264574976).GetUser(discordId).Roles.Where(x => x.Id == id).FirstOrDefault() != default)
                        return true;
                }
            }
            return false;
        }
        #endregion

        #region Empty roles
        private void WriteEmptyRoles()
        {
            string message = "Пустые роли:\n";
            int count = 0;
            foreach (SocketRole role in DiscordSocketClient.GetGuild(351766372264574976).Roles)
            {
                if (role.Members.Count() == 0)
                {
                    count++;
                    message = message + "\n" + count + ") " + role.Mention;
                }
            }
            DiscordSocketClient.GetGuild(351766372264574976).GetTextChannel(689181158730104973).SendMessageAsync(message + "\n");
        }
        #endregion
    }
}