using System.Collections.Generic;

namespace InternalService.Logs
{
    public class Server
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LogsPath { get; set; }
        public List<Log> Logs { get; set; }
    }
}