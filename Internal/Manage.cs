using Discord;
using Discord.WebSocket;
using InternalService.Main;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternalService.Internal
{
    public static class Manage
    {
        public static readonly Dictionary<ulong, Report> DiscordUsersReportDialogue = new Dictionary<ulong, Report>();

        public static string[] GetReportMessage(Report report)
        {
            string reportmessage = string.Empty;
            ulong reportid = GetReportIdNext();
            SaveLoad.SaveReportId();
            reportmessage = reportmessage + "Номер: " + reportid + "\n";
            reportmessage = reportmessage + "Подозреваемый: " + report.Suspect + "\n";
            reportmessage = reportmessage + "Нарушение: " + report.Reason + "\n";
            reportmessage = reportmessage + "Доказательства:\n" + report.Proof + "\n";
            return new string[] { reportmessage, reportid.ToString() };
        }

        public static string GetFullReportMessage(Report report, ulong reporterId, ulong reportid)
        {
            string reportmessage = string.Empty;
            reportmessage = reportmessage + "Номер: " + reportid + "\n";
            reportmessage = reportmessage + "Отправитель: " + report.Sender + "\n";
            reportmessage = reportmessage + "Уникальный идентификатор отправителя в дискорде: " + reporterId.ToString() + "\n";
            reportmessage = reportmessage + "Подозреваемый: " + report.Suspect + "\n";
            reportmessage = reportmessage + "Нарушение: " + report.Reason + "\n";
            reportmessage = reportmessage + "Доказательства:\n" + report.Proof + "\n";
            return reportmessage;
        }

        public static string GetReportMessagePreView(Report report)
        {
            string reportmessage = string.Empty;
            reportmessage = reportmessage + "**Начало жалобы**" + "\n";
            reportmessage = reportmessage + "Подозреваемый: " + report.Suspect + "\n";
            reportmessage = reportmessage + "Нарушение: " + report.Reason + "\n";
            reportmessage = reportmessage + "Доказательства:\n" + report.Proof + "\n";
            reportmessage = reportmessage + "**Конец жалобы**" + "\n";
            return reportmessage;
        }

        public static ulong ReportId = 0;

        private static ulong GetReportIdNext()
        {
            ReportId++;
            return ReportId;
        }

        public static async Task OnMessageReceived(SocketMessage arg)
        {
            if (arg.Content is null || arg.Author.IsBot)
            {
                return;
            }

            if (arg.Channel.Id == Info.ManageChannelId)
            {
                if (arg.Content.ToLower().Contains((Info.Prefix + Info.AddReporter).ToLower()) && arg.Content.ToLower().IndexOf((Info.Prefix + Info.AddReporter).ToLower()) == 0)
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
                                await arg.Channel.SendMessageAsync(Info.SuccessAdd);
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
                else if (arg.Content.ToLower().Contains((Info.Prefix + Info.RemoveReporter).ToLower()) && arg.Content.ToLower().IndexOf((Info.Prefix + Info.RemoveReporter).ToLower()) == 0)
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
                                await arg.Channel.SendMessageAsync(Info.SuccessRemove);
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
                else if (arg.Content.ToLower().Contains((Info.Prefix + Info.ListReporter).ToLower()) && arg.Content.ToLower().IndexOf((Info.Prefix + Info.ListReporter).ToLower()) == 0)
                {
                    string answer = string.Empty;
                    foreach (ulong id in Info.Reporters)
                    {
                        if (Main.Manage.DiscordSocketClient.GetUser(id) != null)
                        {
                            answer = answer + Main.Manage.DiscordSocketClient.GetUser(id).Mention + "\n";
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


            if (Info.Reporters.Contains(userdiscordid))
            {
                if (DiscordUsersReportDialogue.ContainsKey(userdiscordid))
                {
                    switch (DiscordUsersReportDialogue[userdiscordid].ReportStage)
                    {
                        case ReportStage.SuspectQuestion:
                            DiscordUsersReportDialogue[userdiscordid].Suspect = arg.Content;
                            DiscordUsersReportDialogue[userdiscordid].ReportStage = ReportStage.ReasonQuestion;
                            await arg.Channel.SendMessageAsync(Info.QuestionsReport[DiscordUsersReportDialogue[userdiscordid].ReportStage]);
                            break;
                        case ReportStage.ReasonQuestion:
                            DiscordUsersReportDialogue[userdiscordid].Reason = arg.Content;
                            DiscordUsersReportDialogue[userdiscordid].ReportStage = ReportStage.ProofQuestion;
                            await arg.Channel.SendMessageAsync(Info.QuestionsReport[DiscordUsersReportDialogue[userdiscordid].ReportStage]);
                            break;
                        case ReportStage.ProofQuestion:

                            if (arg.Content.ToLower() == (Info.Prefix + Info.ProofKeyWordEnd).ToLower())
                            {
                                DiscordUsersReportDialogue[userdiscordid].ReportStage = ReportStage.ConfirmSendQuestion;
                                await arg.Channel.SendMessageAsync(GetReportMessagePreView(DiscordUsersReportDialogue[userdiscordid]) + "\n" + Info.ConfirmSendQuestion);
                            }
                            else
                            {
                                DiscordUsersReportDialogue[userdiscordid].Proof = DiscordUsersReportDialogue[userdiscordid].Proof + arg.Content + "\n";
                                foreach (Attachment attachment in arg.Attachments.ToList())
                                {
                                    DiscordUsersReportDialogue[userdiscordid].Proof = DiscordUsersReportDialogue[userdiscordid].Proof + attachment.ProxyUrl + "\n";
                                }
                            }
                            break;
                        case ReportStage.ConfirmSendQuestion:
                            if (arg.Content.ToLower() == Info.ConfirmSendAnswerYes.ToLower() || arg.Content.ToLower() == "1")
                            {
                                string[] result = GetReportMessage(DiscordUsersReportDialogue[userdiscordid]);
                                await (Main.Manage.DiscordSocketClient.GetGuild(Info.TribunalServerId).GetChannel(Info.ReportChannelId) as IMessageChannel).SendMessageAsync(result[0]);
                                await (Main.Manage.DiscordSocketClient.GetGuild(Info.TribunalServerId).GetChannel(Info.ReportFullChannelId) as IMessageChannel).SendMessageAsync(GetFullReportMessage(DiscordUsersReportDialogue[userdiscordid], arg.Author.Id, ulong.Parse(result[1])));
                                await arg.Channel.SendMessageAsync("Ваша жалоба успешно отправлена.");
                                DiscordUsersReportDialogue.Remove(userdiscordid);
                            }
                            else if (arg.Content.ToLower() == Info.ConfirmSendAnswerNo.ToLower() || arg.Content.ToLower() == "2")
                            {
                                string[] result = GetReportMessage(DiscordUsersReportDialogue[userdiscordid]);
                                await (Main.Manage.DiscordSocketClient.GetGuild(Info.TribunalServerId).GetChannel(Info.LogsChannelId) as IMessageChannel).SendMessageAsync(Info.HiddenMessage + GetFullReportMessage(DiscordUsersReportDialogue[userdiscordid], arg.Author.Id, ulong.Parse(result[1])));
                                await arg.Channel.SendMessageAsync("Ваша жалоба не отправлена.");
                                DiscordUsersReportDialogue.Remove(userdiscordid);
                            }
                            else
                            {
                                await arg.Channel.SendMessageAsync(Info.ConfirmSendQuestion);
                            }
                            break;
                        default:
                            DiscordUsersReportDialogue.Remove(userdiscordid);
                            return;
                    }
                }
                else
                {
                    if (arg.Content.ToLower() == (Info.Prefix + Info.Report).ToLower())
                    {
                        DiscordUsersReportDialogue[userdiscordid] = new Report()
                        {
                            ReportStage = ReportStage.SuspectQuestion,
                            Sender = arg.Author.Mention
                        };
                        await arg.Author.SendMessageAsync(Info.QuestionsReport[DiscordUsersReportDialogue[userdiscordid].ReportStage]);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
    }
}

public enum ReportStage
{
    SuspectQuestion, ReasonQuestion, ProofQuestion, ConfirmSendQuestion
}

public enum LogsDialogueStage
{
    Server, Day, Current
}