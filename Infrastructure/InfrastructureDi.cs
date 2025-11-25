using BuildingBlocks.Commons;
using Domain.Entities;
using Infrastructure.Background;
using Infrastructure.Database;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SqlKata.Compilers;
using StackExchange.Redis;
using Serilog;

namespace Infrastructure;

public static class IdentityInfrastructureDi
{
    public static void AddInfrastructure(this IServiceCollection services, string? postgresConnectionString,
        string? redisConnectionString)
    {
        Log.Information("Using connection string: {ConnectionString}", postgresConnectionString);

        try
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(postgresConnectionString));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to configure ApplicationDbContext: {ErrorMessage}", ex.Message);
            throw;
        }

        services.AddIdentityCore<User>(options =>
            {
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            if (string.IsNullOrEmpty(redisConnectionString))
            {
                Log.Error("Redis connection string is null or empty");
                throw new BusinessException("Redis connection string cannot be null or empty");
            }

            var configOptions = ConfigurationOptions.Parse(redisConnectionString);
            return ConnectionMultiplexer.Connect(configOptions);
        });

        services.AddSingleton<HttpClient>();
        services.AddSingleton<PostgresCompiler>();
        services.AddScoped<PostgreSqlServer>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IProgressRepository, ProgressRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();
        services.AddScoped<ICauseTypeRepository, CauseTypeRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IHistoryRepository, HistoryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<QueuedHostService>();
        
    }
}