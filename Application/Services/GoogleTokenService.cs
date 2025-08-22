using System.Text.Json;
using Application.DTOs;
using BuildingBlocks.Settings;
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

    public GoogleTokenService(IOptions<GoogleOAuthSettings> googleOAuthOptions, HttpClient httpClient)
    {
        _googleOAuthSettings = googleOAuthOptions.Value;
        _httpClient = httpClient;
    }

    public async Task<string?> RefreshAccessTokenAsync(string refreshToken)
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
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<GoogleTokenResponse>(json);
        return tokenData?.access_token;
    }
}