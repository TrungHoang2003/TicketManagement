using BuildingBlocks.EmailHelper;
using MailKit.Net.Proxy;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Application.Services;

public interface IEmailService
{
    Task SendTicketNotificationAsync(
        string receiverEmail, 
        string receiverName, 
        string ticketTitle, 
        int ticketId, 
        string creatorName, 
        string priority);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendTicketNotificationAsync(
        string receiverEmail, 
        string receiverName, 
        string ticketTitle, 
        int ticketId, 
        string creatorName, 
        string priority)
    {
        await Task.CompletedTask;
    }

    private async Task SendEmailAsync(
        string receiverEmail, 
        string subject, 
        string body, 
        string creatorName, 
        bool isHtml)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(creatorName, _emailSettings.Username));
            message.To.Add(new MailboxAddress("", receiverEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = isHtml ? body : null,
                TextBody = !isHtml ? body : null
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            // Tăng timeout cho kết nối
            client.Timeout = 30000; // 30 seconds

            // Cấu hình proxy
            if (_emailSettings.UseSystemProxy)
            {
                // Sử dụng system proxy từ environment variables
                var httpProxy = Environment.GetEnvironmentVariable("HTTP_PROXY") ?? Environment.GetEnvironmentVariable("http_proxy");
                if (!string.IsNullOrEmpty(httpProxy))
                {
                    var proxyUri = new Uri(httpProxy);
                    client.ProxyClient = new HttpProxyClient(proxyUri.Host, proxyUri.Port);
                }
            }
            else if (_emailSettings.UseProxy && !string.IsNullOrEmpty(_emailSettings.ProxyHost))
            {
                try 
                {
                    HttpProxyClient proxyClient;
                    
                    // Nếu có username/password cho proxy
                    if (!string.IsNullOrEmpty(_emailSettings.ProxyUsername))
                    {
                        var credentials = new System.Net.NetworkCredential(_emailSettings.ProxyUsername, _emailSettings.ProxyPassword);
                        proxyClient = new HttpProxyClient(_emailSettings.ProxyHost, _emailSettings.ProxyPort, credentials);
                    }
                    else
                    {
                        proxyClient = new HttpProxyClient(_emailSettings.ProxyHost, _emailSettings.ProxyPort);
                    }
                    
                    client.ProxyClient = proxyClient;
                }
                catch (Exception proxyEx)
                {
                    throw new Exception($"Failed to configure proxy {_emailSettings.ProxyHost}:{_emailSettings.ProxyPort}: {proxyEx.Message}", proxyEx);
                }
            }

            await client.ConnectAsync(
                _emailSettings.Host,
                _emailSettings.Port,
                _emailSettings.Port == 465 ? MailKit.Security.SecureSocketOptions.SslOnConnect : 
                MailKit.Security.SecureSocketOptions.None
            );

            await client.AuthenticateAsync(
                _emailSettings.Username,
                _emailSettings.Password
            );

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to send email: {ex.Message}", ex);
        }
    }
}
