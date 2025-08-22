using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BuildingBlocks.Commons;
using BuildingBlocks.EmailHelper;
using Google.Apis.Gmail.v1.Data;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public interface IEmailService
{
    Task SendTicketNotificationAsync(
        int creatorId,
        string receiverEmail, 
        string receiverName, 
        string ticketTitle, 
        int ticketId, 
        string creatorName, 
        string creatorEmail,
        string priority);
}

public class EmailService(IRedisService redis, IGoogleTokenService googleTokenService, ILogger<EmailService> logger) : IEmailService
{
    private readonly HttpClient _httpClient = new();

    public async Task SendTicketNotificationAsync(
        int creatorId,
        string receiverEmail, 
        string receiverName, 
        string ticketTitle, 
        int ticketId, 
        string creatorName, 
        string creatorEmail,
        string priority)
    {
        var subject = $"🎫 New Ticket Created: {ticketTitle}";
        var body = EmailTemplates.GetTicketNotificationTemplate(
            receiverName, 
            ticketTitle, 
            ticketId, 
            creatorName,
            priority);

        await SendEmailAsync(creatorId, creatorEmail, receiverEmail, subject, body);
    }

    private async Task SendEmailAsync(int userId, string fromEmail, string toEmail, string subject,
        string htmlBody)
    {

        // Lấy refresh token
        var refreshKey = $"refreshToken:{userId}";
        var refreshToken = await redis.GetValue(refreshKey);

        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new BusinessException($"Refresh token not found for user {userId}");
        }

        // Lấy access token
        var accessToken = await googleTokenService.RefreshAccessTokenAsync(refreshToken);

        if (string.IsNullOrEmpty(accessToken))
        {
            throw new BusinessException($"Failed to refresh access token for user {userId}");
        }

        // Tạo và gửi email
        var rawMessage = CreateRawMessage(fromEmail, toEmail, subject, htmlBody);
        await SendEmailViaGmailApi(accessToken, rawMessage);
    }

    private async Task SendEmailViaGmailApi(string accessToken, string rawMessage)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var json = JsonSerializer.Serialize(new { raw = rawMessage });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://gmail.googleapis.com/gmail/v1/users/me/messages/send", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new BusinessException(errorContent);
            }

            logger.LogInformation("Email sent successfully via Gmail API");
        }
        catch (Exception ex)
        {
            throw new BusinessException(ex.Message);
        }
    }

    private string CreateRawMessage(string from, string to, string subject, string body)
    {
        var message = new StringBuilder();
        message.AppendLine($"From: {from}");
        message.AppendLine($"To: {to}");
        message.AppendLine($"Subject: {EncodeSubject(subject)}");
        message.AppendLine("MIME-Version: 1.0");
        message.AppendLine("Content-Type: text/html; charset=utf-8");
        message.AppendLine();
        message.AppendLine(body);

        var bytes = Encoding.UTF8.GetBytes(message.ToString());
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }

    private string EncodeSubject(string subject)
    {
        var bytes = Encoding.UTF8.GetBytes(subject);
        return "=?utf-8?B?" + Convert.ToBase64String(bytes) + "?=";
    }

}