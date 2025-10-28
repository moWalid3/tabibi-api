
using Scalar.AspNetCore;
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
                .AddEmailServices();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                await app.SeedInitialDataAsync();
            }

            app.MapOpenApi();
            app.MapScalarApiReference();

            app.UseHttpsRedirection();

            app.UseExceptionHandler();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
