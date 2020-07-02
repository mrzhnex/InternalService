using System.Collections.Generic;

namespace InternalService
{
    public static class Session
    {
        public static readonly Dictionary<ulong, Report> DiscordUsersReportDialogue = new Dictionary<ulong, Report>();

        public static readonly Dictionary<ulong, LogsDialogue> DiscordUsersLogsDialogue = new Dictionary<ulong, LogsDialogue>();

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