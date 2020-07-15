using InternalService.Manage;
using System;
using System.IO;
using System.Threading;

namespace InternalService.FullRP
{
    public static class PlayersWhoPlayedMethods
    {
        public static void SendPlayersWhoPlayedMessage(string message)
        {
            Program.ProgramSession.DiscordSocketClient.GetGuild(351766372264574976).GetTextChannel(639060639070814208).SendMessageAsync(message);
        }
        public static void CheckForUpdatePlayersWhoPlayedFile()
        {
            try
            {
                if (!File.Exists(Path.Combine(PlayersWhoPlayed.Global.FilePath, PlayersWhoPlayed.Global.FileName)))
                    File.Create(Path.Combine(PlayersWhoPlayed.Global.FilePath, PlayersWhoPlayed.Global.FileName));
                string message = File.ReadAllText(Path.Combine(PlayersWhoPlayed.Global.FilePath, PlayersWhoPlayed.Global.FileName), System.Text.Encoding.UTF8);
                if (message != PlayersWhoPlayed.Global.VoidData)
                {
                    SendPlayersWhoPlayedMessage(message);
                    File.WriteAllText(Path.Combine(PlayersWhoPlayed.Global.FilePath, PlayersWhoPlayed.Global.FileName), PlayersWhoPlayed.Global.VoidData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Catch exception: " + ex.Message);
            }
        }
        public static void LoopCheckForUpdate()
        {
            while (Global.Active)
            {
                Thread.Sleep(10000);
                CheckForUpdatePlayersWhoPlayedFile();
            }
        }
    }
}
