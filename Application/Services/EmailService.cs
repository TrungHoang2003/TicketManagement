using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.DTOs;
using BuildingBlocks.Commons;
using BuildingBlocks.EmailHelper;
using Google.Apis.Gmail.v1.Data;
using Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public interface IEmailService
{
    Task SenTicketEmail(SendTicketEmailDto dto);
}

public class EmailService(IRedisService redis, IGoogleTokenService googleTokenService, ILogger<EmailService> logger, IUserRepository userRepo) : IEmailService
{
    private readonly HttpClient _httpClient = new();
    
    public async Task SenTicketEmail(SendTicketEmailDto dto)
    {
        string subject;
        string body;
        switch (dto.Header)
        {
           case EmailHeader.Created:
               subject = $"🎫 New Ticket Created: {dto.TicketTitle}"; 
               body = EmailTemplates.GetTicketCreatedTemplate(dto.ReceiverName, dto.TicketTitle,dto.TicketId, dto.CreatorName, dto.Priority);
               break;
           case EmailHeader.Assigned:
               subject = $"🎫 New Ticket Assigned to you: {dto.TicketTitle}";
               body = EmailTemplates.GetTicketAssignedTemplate(dto.ReceiverName, dto.TicketTitle,dto.TicketId, dto.CreatorName, dto.Priority, dto.Note ?? "");
               break;
           default:
               throw new ArgumentOutOfRangeException();
        }
        await SendEmailAsync(dto.ReceiverEmail, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject,
        string htmlBody)
    {
        // Lấy refresh token của admin
        var admin = await userRepo.GetAdmin();
        
        var refreshKey = $"refreshToken:{admin.Id}";
        var refreshToken = await redis.GetValue(refreshKey);

        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new BusinessException($"Refresh token not found for user {admin.Id}");
        }

        // Lấy access token
        var accessToken = await googleTokenService.RefreshAccessTokenAsync(refreshToken);

        if (string.IsNullOrEmpty(accessToken))
        {
            throw new BusinessException($"Failed to refresh access token for user {admin.Id}");
        }

        // Tạo và gửi email
        var rawMessage = CreateRawMessage(admin.Email!, toEmail, subject, htmlBody);
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