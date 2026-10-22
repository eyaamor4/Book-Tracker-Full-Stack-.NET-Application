using BackendTest.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendTest.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
       public  DbSet<Book> Books { get; set; }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<Book>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedDateTime = now;
                    entry.Entity.ModifiedDateTime = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModifiedDateTime = now;
                    // CreatedDateTime is not editable: never overwritten
                    entry.Property(b => b.CreatedDateTime).IsModified = false;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}