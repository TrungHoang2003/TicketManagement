using System.Threading.Channels;
using Application.DTOs;
using Infrastructure.Background;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public interface IEmailBackgroundService
{
    Task QueueEmailAsync(SendTicketEmailDto emailDto);
}

public class EmailBackgroundService(IServiceProvider serviceProvider, ILogger<EmailBackgroundService> logger) 
    : ScopedBackgroundService(serviceProvider, logger), IEmailBackgroundService
{
    private readonly Channel<SendTicketEmailDto> _emailQueue = Channel.CreateUnbounded<SendTicketEmailDto>();

    public async Task QueueEmailAsync(SendTicketEmailDto emailDto)
    {
        await _emailQueue.Writer.WriteAsync(emailDto);
        Logger.LogInformation("Email queued for ticket {TicketId}", emailDto.TicketId);
    }

    protected override async Task ExecuteScopedAsync(IServiceProvider scopedServiceProvider, CancellationToken stoppingToken)
    {
        await foreach (var emailDto in _emailQueue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                // Resolve EmailService trong scope riêng biệt
                var emailService = scopedServiceProvider.GetRequiredService<IEmailService>();
                await emailService.SenTicketEmail(emailDto);
                
                Logger.LogInformation("Email sent successfully for ticket {TicketId}", emailDto.TicketId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to send email for ticket {TicketId}. Email notification skipped but ticket was created successfully.", emailDto.TicketId);
                
                // Don't retry immediately - just log and continue
                // This prevents the system from blocking if email service is unavailable
                // In production, you might want to implement a retry queue with exponential backoff
            }
        }
    }
}
