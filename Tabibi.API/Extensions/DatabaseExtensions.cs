using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tabibi.API.Common;
using Tabibi.API.Database;
using Tabibi.API.Entities;

namespace Tabibi.API.Extensions
{
    public static class DatabaseExtensions
    {
        public static async Task SeedInitialDataAsync(this WebApplication app)
        {
            await using AsyncServiceScope scope = app.Services.CreateAsyncScope();

            var serviceProvider = scope.ServiceProvider;

            try
            {
                AppDbContext context = serviceProvider.GetRequiredService<AppDbContext>();
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                throw;
            }

            using RoleManager<IdentityRole> roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            using UserManager<ApplicationUser> userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = [Roles.Admin, Roles.Doctor, Roles.Patient];

            foreach (string role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            string adminEmail = "admin@gmail.com";
            string adminPassword = "Admin@123";

            ApplicationUser? userExist = await userManager.FindByEmailAsync(adminEmail);

            if (userExist == null)
            {
                ApplicationUser adminUser = new()
                {
                    Name = "Admin",
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    CreatedAtUtc = DateTime.UtcNow
                };

                IdentityResult result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                }
                else
                {
                    throw new Exception("Failed to create the admin user: " + string.Join(", ", result.Errors));
                }
            }
        }
    }
}
