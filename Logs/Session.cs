using System.Collections.Generic;

namespace InternalService.Logs
{
    public static class Session
    {
        public static readonly Dictionary<ulong, LogsDialogue> DiscordUsersLogsDialogue = new Dictionary<ulong, LogsDialogue>();
    }
}