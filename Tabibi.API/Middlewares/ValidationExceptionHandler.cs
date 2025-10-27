using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace Tabibi.API.Middlewares
{
    public sealed class ValidationExceptionHandler
        (IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not ValidationException validationException)
            {
                return false;
            }

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            ProblemDetailsContext context = new()
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new()
                {
                    Detail = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest
                }
            };

            Dictionary<string, string[]> errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key.ToLower(),
                    g => g.Select(e => e.ErrorMessage).ToArray());

            context.ProblemDetails.Extensions.Add("errors", errors);

            return await problemDetailsService.TryWriteAsync(context);
        }
    }
}
