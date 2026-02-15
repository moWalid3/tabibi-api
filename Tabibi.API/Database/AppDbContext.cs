using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tabibi.API.Entities;

namespace Tabibi.API.Database
{
    public sealed class AppDbContext
        (DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<WorkSchedule> WorkSchedules { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema(Schemas.Identity);

            builder.Entity<Clinic>().ToTable(nameof(Clinics), Schemas.Core);
            builder.Entity<Department>().ToTable(nameof(Departments), Schemas.Core);
            builder.Entity<City>().ToTable(nameof(Cities), Schemas.Core);
            builder.Entity<WorkSchedule>().ToTable(nameof(WorkSchedules), Schemas.Core);
            builder.Entity<Booking>().ToTable(nameof(Bookings), Schemas.Core);
            builder.Entity<Review>().ToTable(nameof(Reviews), Schemas.Core);
            builder.Entity<Notification>().ToTable(nameof(Notifications), Schemas.Core);
            builder.Entity<ChatMessage>().ToTable(nameof(ChatMessages), Schemas.Core);
            builder.Entity<Prescription>().ToTable(nameof(Prescriptions), Schemas.Core);
            builder.Entity<PrescriptionItem>().ToTable(nameof(PrescriptionItems), Schemas.Core);
            builder.Entity<Favorite>().ToTable(nameof(Favorites), Schemas.Core);

            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
