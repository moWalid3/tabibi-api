using Scalar.AspNetCore;
using Tabibi.API.Configurations;
using Tabibi.API.Extensions;
using Tabibi.API.Hubs;

namespace Tabibi.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder
                .AddApiServices()
                .AddErrorHandling()
                .AddDatabase()
                .AddApplicationServices()
                .AddAuthenticationServices()
                .AddSortServices()
                .AddEmailServices()
                .AddCorsPolicy()
                .AddStripe()
                .AddFirebase()
                .AddHangfire();

            var app = builder.Build();

            await app.SeedInitialDataAsync();

            app.MapOpenApi();
            app.MapScalarApiReference();

            app.UseHttpsRedirection();

            app.UseExceptionHandler();

            app.UseStaticFiles();

            app.UseCors(CorsOptions.PolicyName);

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.MapHub<MainHub>("/hub");

            app.Run();
        }
    }
}
