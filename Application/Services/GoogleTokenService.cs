using System.Text.Json;
using Application.DTOs;
using BuildingBlocks.Settings;
 using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services;

public interface IGoogleTokenService
{
    Task<string?> RefreshAccessTokenAsync(string refreshToken);
}

public class GoogleTokenService: IGoogleTokenService
{
    private readonly GoogleOAuthSettings _googleOAuthSettings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleTokenService> _logger;

    public GoogleTokenService(IOptions<GoogleOAuthSettings> googleOAuthOptions, HttpClient httpClient, ILogger<GoogleTokenService> logger)
    {
        _googleOAuthSettings = googleOAuthOptions.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> RefreshAccessTokenAsync(string refreshToken)
    {
        try
        {
            var values = new Dictionary<string, string>
            {
                { "client_id", _googleOAuthSettings.ClientId },
                { "client_secret", _googleOAuthSettings.ClientSecret },
                { "refresh_token", refreshToken },
                { "grant_type", "refresh_token" }
            };

            var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(values));
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to refresh Google OAuth token. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<GoogleTokenResponse>(json);
            
            if (tokenData?.access_token != null)
            {
                _logger.LogInformation("Successfully refreshed Google OAuth access token");
            }
            
            return tokenData?.access_token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while refreshing Google OAuth token");
            return null;
        }
    }
}