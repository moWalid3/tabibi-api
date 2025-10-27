using Microsoft.AspNetCore.Diagnostics;

namespace Tabibi.API.Middlewares
{
    public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        public ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            return problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new()
                {
                    Title = "Internal server error",
                    Detail = "An error occurred while processing your request."
                }
            });
        }
    }
}
