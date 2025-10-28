using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tabibi.API.Database.Configurations;
using Tabibi.API.Entities;

namespace Tabibi.API.Database
{
    public sealed class AppDbContext
        (DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema(Schemas.Identity);

            ////builder.Entity<Next>().ToTable(nameof(Nexts), Schemas.Core);

            builder.ApplyConfiguration(new ApplicationUserConfiguration());
            builder.ApplyConfiguration(new RefreshTokenConfiguration());
        }
    }
}
