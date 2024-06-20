

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text.Json;

namespace WhosPetUI.ExHandlreMiddleware
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandler> _logger;

        public ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                context.Request.EnableBuffering(); 
                var request = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0; 
                _logger.LogInformation($"Incoming request: {request}");

                await _next(context);
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogError($"Token expired: {ex}");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token has expired");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error.",
                ExceptionMessage = exception.Message,
                StackTrace = exception.StackTrace
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }

}
