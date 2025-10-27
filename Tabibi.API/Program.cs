
using Scalar.AspNetCore;

namespace Tabibi.API
{
    public class Program
    {
        public static void Main(string[] args)
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
