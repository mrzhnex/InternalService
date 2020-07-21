using System.Data.Entity;

namespace InternalService.Administrative
{
    public class FoundationContext : DbContext
    {
        public FoundationContext(string connectionString) : base(connectionString) { }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Post> Posts { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (Database.Exists())
                Database.SetInitializer<FoundationContext>(null);
            base.OnModelCreating(modelBuilder);
        }
    }
}