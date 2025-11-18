using Scalar.AspNetCore;
using Tabibi.API.Configurations;
using Tabibi.API.Extensions;

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
                .AddEmailServices()
                .AddCorsPolicy();

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

            app.Run();
        }
    }
}
