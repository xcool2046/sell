using Sellsys.CrossCutting.Common;
using System.Net;
using System.Text.Json;

namespace Sellsys.WebApi.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = ApiResponse.Fail(
                message: $"Internal Server Error. Please contact support. Error ID: {context.TraceIdentifier}",
                statusCode: HttpStatusCode.InternalServerError);
            
            var jsonResponse = JsonSerializer.Serialize(response);
            
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}