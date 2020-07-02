namespace InternalService
{
    public class LogsDialogue
    {
        public LogsDialogueStage LogsDialogueStage { get; set; }
        public ulong LastSessionMessage { get; set; }
        public Server Server { get; set; }
        public string Date { get; set; }
        public Log Log { get; set; }
    }
}
