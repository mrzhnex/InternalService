using InternalService.Main;
using System;
using System.IO;
using System.Threading;

namespace InternalService.FullRP
{
    public static class Manage
    {
        public static void SendPlayersWhoPlayedMessage(string message)
        {
            Main.Manage.DiscordSocketClient.GetGuild(351766372264574976).GetTextChannel(639060639070814208).SendMessageAsync(message);
        }
        public static void CheckForUpdatePlayersWhoPlayedFile()
        {
            try
            {
                if (!File.Exists(Path.Combine(Info.FilePath, Info.FileName)))
                    File.Create(Path.Combine(Info.FilePath, Info.FileName));
                string message = File.ReadAllText(Path.Combine(Info.FilePath, Info.FileName), System.Text.Encoding.UTF8);
                if (message != Info.VoidData)
                {
                    SendPlayersWhoPlayedMessage(message);
                    File.WriteAllText(Path.Combine(Info.FilePath, Info.FileName), Info.VoidData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Catch exception: " + ex.Message);
            }
        }
        public static void LoopCheckForUpdate()
        {
            while (Main.Info.Active)
            {
                Thread.Sleep(10000);
                CheckForUpdatePlayersWhoPlayedFile();
            }
        }
    }
}
