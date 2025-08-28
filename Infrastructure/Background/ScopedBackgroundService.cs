using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Background;

/// <summary>
/// Background service base class that properly handles scoped dependencies
/// </summary>
public abstract class ScopedBackgroundService(IServiceProvider serviceProvider, ILogger logger) : BackgroundService
{
    protected readonly IServiceProvider ServiceProvider = serviceProvider;
    protected readonly ILogger Logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("{ServiceName} background service started.", GetType().Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = ServiceProvider.CreateScope();
                await ExecuteScopedAsync(scope.ServiceProvider, stoppingToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred in {ServiceName}", GetType().Name);
                
                // Wait a bit before retrying to prevent tight loop on persistent errors
                await Task.Delay(5000, stoppingToken);
            }
        }

        Logger.LogInformation("{ServiceName} background service stopped.", GetType().Name);
    }

    /// <summary>
    /// Execute work with properly scoped dependencies
    /// </summary>
    /// <param name="serviceProvider">Scoped service provider</param>
    /// <param name="stoppingToken">Cancellation token</param>
    /// <returns></returns>
    protected abstract Task ExecuteScopedAsync(IServiceProvider serviceProvider, CancellationToken stoppingToken);
}
