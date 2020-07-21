using System.Collections.Generic;
using System.IO;

namespace InternalService.Internal
{
    public static class Info
    {
        public static readonly string Report = "Report";
        public static readonly string Prefix = ".";

        public static List<ulong> Reporters = new List<ulong>();

        public static readonly string ReportersFileName = "Reporters.xml";
        public static readonly string ReportIdFileName = "ReportId.xml";
        public static readonly string HostDataPath = Path.Combine("/etc/scpsl/Internal");

        public static readonly ulong ReportChannelId = 678908085615591431;
        public static readonly ulong LogsChannelId = 678908863969361920;
        public static readonly ulong ManageChannelId = 678909048225005569;
        public static readonly ulong ReportFullChannelId = 678916367637938196;
        public static readonly ulong TribunalServerId = 678659777550745608;

        private static readonly string SuspectQuestion = "На кого вы хотите пожаловаться?";
        private static readonly string ReasonQuestion = "Что нарушил?";
        private static readonly string ProofQuestion = "Доказательства?";

        public static readonly string ProofKeyWordEnd = "End";
        public static readonly int ProofLimit = 5;

        public static readonly Dictionary<ReportStage, string> QuestionsReport = new Dictionary<ReportStage, string>()
        {
            {ReportStage.SuspectQuestion, SuspectQuestion },
            {ReportStage.ReasonQuestion, ReasonQuestion },
            {ReportStage.ProofQuestion, ProofQuestion }
        };

        public static readonly Dictionary<LogsDialogueStage, string> QuestionsLogs = new Dictionary<LogsDialogueStage, string>()
        {            
            { LogsDialogueStage.Server, "Выберите сервер из списка." },
            { LogsDialogueStage.Day, "Выберите дату из списка." },
            { LogsDialogueStage.Current, "Выберите лог или напишите " + Prefix + "exit, чтобы выйти из выбора логов." }
        };

        public static readonly string ConfirmSendQuestion = "Вы уверены, что хотите отправить?\n1. Да\n2. Нет";
        public static readonly string HiddenMessage = "**Пользователь отказался отправлять жалобу:**\n";

        public static readonly string ConfirmSendAnswerYes = "Да";
        public static readonly string ConfirmSendAnswerNo = "Нет";


        public static readonly string AddReporter = "Add";
        public static readonly string RemoveReporter = "Remove";
        public static readonly string ListReporter = "List";
        public static readonly string SuccessAdd = "Пользовать успешно добавлен";
        public static readonly string SuccessRemove = "Пользователь успешно удален";
    }
}