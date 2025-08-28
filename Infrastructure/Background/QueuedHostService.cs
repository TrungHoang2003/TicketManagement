using Microsoft.Extensions.Logging;

namespace Infrastructure.Background;

public class QueuedHostService(IBackgroundTaskQueue taskQueue, ILogger<QueuedHostService> logger, IServiceProvider serviceProvider) 
    : ScopedBackgroundService(serviceProvider, logger)
{
    protected override async Task ExecuteScopedAsync(IServiceProvider scopedServiceProvider, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await taskQueue.DequeuedAsync(stoppingToken);
                
                // Thực thi task với scoped services
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred executing background task");
                
                // Continue processing other tasks
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}