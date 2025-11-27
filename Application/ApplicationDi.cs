using Application.Mappings;
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
        services.Configure<OllamaSettings>(configuration.GetSection("Ollama"));
        services.Configure<RagPromptSettings>(configuration.GetSection("RagPrompt"));
        
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IGoogleTokenService, GoogleTokenService>();
        services.AddSingleton<IRedisService, RedisService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IProgressService, ProgressService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ICauseTypeService, CauseTypeService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IRagService, RagService>();
        services.AddHttpClient<IGoogleAuthService, GoogleAuthService>();
        services.AddSingleton<ICloudinaryService,CloudinaryService>();
        
        // Add EmailBackgroundService cho việc gửi email không đồng bộ
        services.AddSingleton<IEmailBackgroundService, EmailBackgroundService>();
        services.AddHostedService<EmailBackgroundService>(provider => 
            (EmailBackgroundService)provider.GetRequiredService<IEmailBackgroundService>());
        
        
        //Add Automapper
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        
        return services;
    }
}