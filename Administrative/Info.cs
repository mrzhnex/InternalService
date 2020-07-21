using System.Data.SqlClient;

namespace InternalService.Administrative
{
    public static class Info
    {
        public static readonly string Prefix = "!";
        public static readonly string UsernameSplitSymbol = ",";
        public static readonly string UnknownErrorMessage = "Произошла неизвестная ошибка.";
        public static readonly string SteamIdIsNotSetMessage = "Идентификатор стима не установлен.";

        public static string ConnectionString = new SqlConnectionStringBuilder
        {
            DataSource = @"(localdb)\MSSQLLocalDB",
            InitialCatalog = "Foundation",
            IntegratedSecurity = true,
            ConnectTimeout = 30,
            Encrypt = false,
            TrustServerCertificate = false,
            ApplicationIntent = ApplicationIntent.ReadWrite,
            MultiSubnetFailover = false
        }.ConnectionString;
    }
}