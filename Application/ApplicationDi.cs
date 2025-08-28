using Application.Services;
using BuildingBlocks.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationDi
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GoogleOAuthSettings>(configuration.GetSection("Authentication:Google"));
        services.Configure<JwtSettings>(configuration.GetSection("JWT"));
        
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IGoogleTokenService, GoogleTokenService>();
        services.AddSingleton<IRedisService, RedisService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IProgressService, ProgressService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddHttpClient<IGoogleAuthService, GoogleAuthService>();
        services.AddSingleton<ICloudinaryService,CloudinaryService>();
        
        // Add EmailBackgroundService cho việc gửi email không đồng bộ
        services.AddSingleton<IEmailBackgroundService, EmailBackgroundService>();
        services.AddHostedService<EmailBackgroundService>(provider => 
            (EmailBackgroundService)provider.GetRequiredService<IEmailBackgroundService>());
        
        return services;
    }
}