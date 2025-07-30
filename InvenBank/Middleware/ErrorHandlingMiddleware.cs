using InvenBank.API.DTOs.Responses;
using System.Net;
using System.Text.Json;

namespace InvenBank.API.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
                _logger.LogError(ex, "Error no manejado en la aplicación: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<object>();

            switch (exception)
            {
                case ValidationException validationEx:
                    response = ApiResponse<object>.ErrorResult(
                        "Error de validación",
                        validationEx.Errors?.ToList() ?? new List<string> { validationEx.Message }
                    );
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case UnauthorizedAccessException:
                    response = ApiResponse<object>.ErrorResult("Acceso no autorizado");
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                case KeyNotFoundException:
                    response = ApiResponse<object>.ErrorResult("Recurso no encontrado");
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case ArgumentException argEx:
                    response = ApiResponse<object>.ErrorResult($"Argumento inválido: {argEx.Message}");
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case InvalidOperationException invalidOpEx:
                    response = ApiResponse<object>.ErrorResult($"Operación inválida: {invalidOpEx.Message}");
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case TimeoutException:
                    response = ApiResponse<object>.ErrorResult("La operación ha excedido el tiempo límite");
                    context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    break;

                default:
                    response = ApiResponse<object>.ErrorResult(
                        "Error interno del servidor. Por favor, contacte al administrador."
                    );
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
