using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tabibi.API.Entities;

namespace Tabibi.API.Database
{
    public sealed class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema(Schemas.Identity);

            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);

                entity.Property(rt => rt.Token).HasMaxLength(1000);
                entity.Property(rt => rt.UserId).HasMaxLength(450);

                entity.HasIndex(rt => rt.Token).IsUnique();

                entity.HasOne(rt => rt.User)
                    .WithMany()
                    .HasForeignKey(rt => rt.UserId);
            });

            SeedRoles(builder);
        }

        public static void SeedRoles(ModelBuilder builder)
        {
            IdentityRole[] roles = [
                new() {
                    Id = "cda9b51a-02db-4882-aa9d-8a10473ece85",
                    Name = Common.Roles.Admin,
                    NormalizedName = Common.Roles.Admin.ToUpper()
                },
                new() {
                    Id = "2f90d769-7d82-4f91-a818-83848e0bd203",
                    Name = Common.Roles.Doctor,
                    NormalizedName = Common.Roles.Doctor.ToUpper()
                },
                new() {
                    Id = "b218d46f-d129-45b4-a1f7-dceab03dc0c6",
                    Name = Common.Roles.Patient,
                    NormalizedName = Common.Roles.Patient.ToUpper()
                },
            ];

            builder.Entity<IdentityRole>().HasData(roles);
        }
    }
}
