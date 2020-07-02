using System.Collections.Generic;

namespace InternalService
{
    public static class KeyWord
    {
        public static readonly string Report = "Report";
        public static readonly string Prefix = ".";

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