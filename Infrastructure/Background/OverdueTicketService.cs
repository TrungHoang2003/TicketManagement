using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Background;

/// <summary>
/// Background service that automatically updates ticket status to Overdue
/// when ExpectedCompleteDate has passed and ticket is not closed
/// </summary>
public class OverdueTicketService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OverdueTicketService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Check every 5 minutes

    public OverdueTicketService(
        IServiceProvider serviceProvider,
        ILogger<OverdueTicketService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Overdue Ticket Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndUpdateOverdueTicketsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking overdue tickets");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Overdue Ticket Service stopped.");
    }

    private async Task CheckAndUpdateOverdueTicketsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;

        // Find tickets that are overdue:
        // - ExpectedCompleteDate has passed
        // - Status is not Closed, Rejected, or already Overdue
        var overdueTickets = await dbContext.Set<Ticket>()
            .Where(t => t.ExpectedCompleteDate.HasValue &&
                       t.ExpectedCompleteDate.Value < now &&
                       t.Status != Status.Closed &&
                       t.Status != Status.Rejected &&
                       t.Status != Status.Overdue)
            .ToListAsync(stoppingToken);

        if (overdueTickets.Count > 0)
        {
            _logger.LogInformation("Found {Count} tickets to mark as overdue", overdueTickets.Count);

            foreach (var ticket in overdueTickets)
            {
                ticket.Status = Status.Overdue;
                
                _logger.LogInformation(
                    "Ticket #{TicketId} '{Title}' marked as Overdue. Expected: {ExpectedDate}",
                    ticket.Id, ticket.Title, ticket.ExpectedCompleteDate);
            }

            await dbContext.SaveChangesAsync(stoppingToken);
            
            _logger.LogInformation("Updated {Count} tickets to Overdue status", overdueTickets.Count);
        }
    }
}
