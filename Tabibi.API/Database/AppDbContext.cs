using Microsoft.EntityFrameworkCore;
using Tabibi.API.Database.Configurations;
using Tabibi.API.Entities;

namespace Tabibi.API.Database
{
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schemas.Application);

            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }
    }
}
