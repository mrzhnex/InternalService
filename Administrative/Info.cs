using System.IO;

namespace InternalService.Administrative
{
    public static class Info
    {
        public static readonly string Prefix = "!";
        public static readonly string UsernameSplitSymbol = ",";
        public static readonly string UnknownErrorMessage = "Произошла неизвестная ошибка.";
        public static readonly string SteamIdIsNotSetMessage = "Идентификатор стима не установлен.";
        public static readonly string DefaultSteamId = "0";
        public static bool IsUpdated = false;
        public static string AdministrationFullFileName 
        { 
            get
            {
                if (Main.Info.IsHost)
                    return Path.Combine("/etc/scpsl/Administrative/", "Administration.txt");
                else
                    return Path.Combine("E:/Info/", "Administration.txt");
            }
            private set { }
        }
        public static string ConnectionString = string.Empty;
    }
}