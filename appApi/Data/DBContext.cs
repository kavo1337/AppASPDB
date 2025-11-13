using Microsoft.EntityFrameworkCore;

namespace appApi.Models
{
    public class DBContext(DbContextOptions<DBContext> options) : DbContext(options)
    {
        public DbSet<Users> Users => Set<Users>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>(e =>
            {
                e.Property(p => p.Name).IsRequired().HasMaxLength(100);
                e.Property(p => p.Balance).HasPrecision(18, 2);
                e.Property(p => p.Age).HasPrecision(3, 0);
            });
        }
    }
}
