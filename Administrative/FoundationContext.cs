using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InternalService.Administrative
{
    public class FoundationContext : DbContext
    {
        public FoundationContext()
        {
            Database.EnsureCreated();
        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(Info.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            List<string> AdministrationGroups = new List<string>();
            foreach (Employee employee in Employees.Include(x => x.Post).ToList())
            {
                if (employee.SteamId == Info.DefaultSteamId)
                    continue;
                AdministrationGroups.Add(" - " + employee.SteamId + "@steam: " + employee.Post.GameRole);
            }
            await File.WriteAllLinesAsync(Info.AdministrationFullFileName, AdministrationGroups);
            await Main.Manage.Log(Main.LogType.Info, "Переопределены записи " + AdministrationGroups.Count + " сотрудников.");
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}