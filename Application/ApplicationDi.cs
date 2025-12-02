using Application.Mappings;
using Application.Services;
using BuildingBlocks.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OllamaSharp;

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
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ICauseTypeService, CauseTypeService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IRagService, RagService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddHttpClient<IGoogleAuthService, GoogleAuthService>();
        services.AddSingleton<ICloudinaryService,CloudinaryService>();
        
        // Add EmailBackgroundService cho việc gửi email không đồng bộ
        services.AddSingleton<IEmailBackgroundService, EmailBackgroundService>();
        services.AddHostedService<EmailBackgroundService>(provider => 
            (EmailBackgroundService)provider.GetRequiredService<IEmailBackgroundService>());
        
        // Đăng ký OllamaApiClient
        services.AddScoped<IOllamaApiClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<OllamaSettings>>().Value;
            var client = new OllamaApiClient(new Uri(settings.BaseUrl));
            // Set model mặc định nếu muốn
            client.SelectedModel = settings.GenerationModel; 
            return client;
        });
        
        //Add Automapper
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        
        return services;
    }
}