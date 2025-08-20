using Application.Shared;
using Serilog;
using Shared.Services;

namespace TicketManagement.Api.Middlewares;

public class TokenValidateMiddleware(RequestDelegate next, RedisService redisService, JwtService jwtService)
{
    public async Task Invoke(HttpContext context)
    {
        Log.Information("Middleware đang xử lý request:  {context.Request.Path}", context.Request.Path);
        
        var path = context.Request.Path.Value?.ToLowerInvariant();

        // Bỏ qua middleware cho các endpoint không cần xác thực
        if (path != null &&
            (path.StartsWith("/authentication/login") ||
             path.StartsWith("/authentication/refreshtoken") ||
             path.StartsWith("/authentication/register") ||
             path.StartsWith("/scalar")))
        {
            await next(context);
            return;
        }

        string? token = null;

        // ✅ Ưu tiên lấy token từ Authorization Header
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            token = authHeader.Substring("Bearer ".Length).Trim();
        }

        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Missing token");
            return;
        }

        try
        {
            var userId = jwtService.GetUserIdFromToken(token);

            // Kiểm tra token lưu trong Redis
            var redisKey = $"accessToken:{userId}";
            var storedToken = await redisService.GetValue(redisKey);
            if (string.IsNullOrEmpty(storedToken) || storedToken != token)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Invalid or expired token");
                return;
            }

            // Lưu userId vào context để sử dụng cho các middleware hoặc controller sau
            context.Items["UserId"] = userId;
            context.Items["AccessToken"] = token;
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: " + ex.Message);
            return;
        }

        // Nếu token hợp lệ, tiếp tục xử lý request
        await next(context);
    }
}