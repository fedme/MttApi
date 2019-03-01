using Microsoft.EntityFrameworkCore;

namespace MttApi.Models 
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Record> Records { get; set; }
        public DbSet<BlacklistedWorker> BlacklistedWorkers { get; set; }
        public DbSet<Condition> Conditions { get; set; }
    }
}