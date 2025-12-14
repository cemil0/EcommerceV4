using System.Net;
using System.Text.Json;

namespace ECommerce.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ErrorResponse
        {
            Timestamp = DateTime.UtcNow,
            Path = context.Request.Path
        };

        switch (exception)
        {
            // Custom Business Exceptions with Error Codes
            case ECommerce.Application.Exceptions.StockNotAvailableException stockEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Success = false;
                response.Message = stockEx.Message;
                response.ErrorCode = stockEx.ErrorCode; // STOCK_1001
                break;

            case ECommerce.Application.Exceptions.PriceChangedException priceEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Success = false;
                response.Message = priceEx.Message;
                response.ErrorCode = priceEx.ErrorCode; // PRICE_2001
                response.Details = new { PriceChanges = priceEx.PriceChanges };
                break;

            case ECommerce.Application.Exceptions.InvalidStateTransitionException stateEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Success = false;
                response.Message = stateEx.Message;
                response.ErrorCode = stateEx.ErrorCode; // ORDER_3001
                response.Details = new 
                { 
                    FromState = stateEx.FromState.ToString(), 
                    ToState = stateEx.ToState.ToString() 
                };
                break;

            case ECommerce.Application.Exceptions.OrderBusinessRuleException businessEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Success = false;
                response.Message = businessEx.Message;
                response.ErrorCode = businessEx.ErrorCode; // ORDER_4001-4005
                break;

            // Generic Framework Exceptions
            case KeyNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Success = false;
                response.Message = "Kayıt bulunamadı";
                response.ErrorCode = "NOT_FOUND";
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Success = false;
                response.Message = "Yetkisiz erişim";
                response.ErrorCode = "UNAUTHORIZED";
                break;

            case ArgumentException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Success = false;
                response.Message = exception.Message;
                response.ErrorCode = "INVALID_ARGUMENT";
                break;

            case InvalidOperationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Success = false;
                response.Message = exception.Message;
                response.ErrorCode = "INVALID_OPERATION";
                break;

            case NotImplementedException:
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                response.Success = false;
                response.Message = "Bu özellik henüz uygulanmadı";
                response.ErrorCode = "NOT_IMPLEMENTED";
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Success = false;
                response.Message = _env.IsDevelopment() 
                    ? exception.Message 
                    : "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.";
                response.ErrorCode = "INTERNAL_ERROR";
                
                // Include stack trace only in development
                if (_env.IsDevelopment())
                {
                    response.Details = new
                    {
                        exception.Message,
                        exception.StackTrace,
                        InnerException = exception.InnerException?.Message
                    };
                }
                break;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(json);
    }
}

public class ErrorResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public object? Details { get; set; }
    public DateTime Timestamp { get; set; }
    public string Path { get; set; } = string.Empty;
}
