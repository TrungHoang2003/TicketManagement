using Application.Services;
using Application.Shared;
using BuildingBlocks.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Services;

namespace Application;

public static class ApplicationDi
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure GoogleOAuth settings
        services.Configure<GoogleOAuthSettings>(configuration.GetSection("GoogleOAuth"));
        
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IRedisService, RedisService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddHttpClient<IGoogleAuthService, GoogleAuthService>();
        
        return services;
    }
}