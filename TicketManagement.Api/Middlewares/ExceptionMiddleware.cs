using System.Text.Json;
using BuildingBlocks.Commons;

namespace TicketManagement.Api.Middlewares;
public class ExceptionHandlerMiddleWare(RequestDelegate next, ILogger<ExceptionHandlerMiddleWare> logger)
{
   public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(httpContext, ex);
        }
    }
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, error) = exception switch
        {
            BusinessException businessEx => (StatusCodes.Status400BadRequest, 
                new Error(businessEx.ErrorCode, businessEx.Message)),
            
            ValidationException validationEx => (StatusCodes.Status400BadRequest, 
                new Error("VALIDATION_ERROR", $"One or more validation errors occurred: {validationEx.Errors}")),
            
            _
                => (StatusCodes.Status500InternalServerError, 
                    new Error("Inner Server Error", exception.Message))
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        
        var result = Result.Failure(error);
        
        var response = new
        {
            result.Success,
            Error = new
            {
                result.Error?.Code,
                result.Error?.Description,
                StackTrace = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" 
                    ? exception.StackTrace : null
            }
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(json);
    }
}