using System.Text.Json;
using Application.DTOs;
using Application.Erros;
using BuildingBlocks.Commons;
using BuildingBlocks.Settings;
using Domain.Entities;
using Google.Apis.Auth;
using Infrastructure.Repositories;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Application.Services;

public interface IGoogleAuthService
{
    string GetGoogleAuthUrl();
    Task<Result<string>> GoogleCallBack(string code);
}


public class GoogleAuthService(IOptions<GoogleOAuthSettings> googleOAuthOptions, HttpClient httpClient, 
    IUserRepository userRepository, IJwtService jwtService, IRedisService redis): IGoogleAuthService
{
    private readonly GoogleOAuthSettings _googleOAuthSettings = googleOAuthOptions.Value;

    public string GetGoogleAuthUrl()
    {
        if (string.IsNullOrEmpty(_googleOAuthSettings.ClientId))
            throw new BusinessException("Google Client ID not found");

        const string scope = "openid profile email https://www.googleapis.com/auth/gmail.send";
        var state = Guid.NewGuid().ToString();

        return $"https://accounts.google.com/o/oauth2/v2/auth?" +
               $"client_id={_googleOAuthSettings.ClientId}&" +
               $"redirect_uri={_googleOAuthSettings.RedirectUri}&" +
               $"response_type=code&" +
               $"scope={scope}&" +
               $"access_type=offline&" +
               $"state={state}";
    }

    public async Task<Result<string>> GoogleCallBack(string code)
    {
        var validateResult = await ValidateGoogleCodeAsync(code);
        if (!validateResult.Success)
            return validateResult.Error!;

        var payload = validateResult.Data;
        var refreshToken = validateResult.SecondData;
        
        var user = await userRepository.FindByEmailAsync(payload.Email);
        
        if (user == null) throw new UnauthorizedAccessException("Tài khoản chưa được cấp, vui lòng liên hệ admin");
        
        user.AvatarUrl = payload.Picture;
        user.EmailConfirmed = true;

        var result = await userRepository.UpdateAsync(user); // No password
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return new Error("Login Failed", string.Join(",", errors));
        }
            
        var roles = await userRepository.GetRolesAsync(user);
        var jwtToken = jwtService.GenerateJwtToken(user, roles);

        var refreshKey = $"refreshToken:{user.Id}";
        var accessKey = $"accessToken:{user.Id}";

        var existingRefreshToken = await redis.GetValue(refreshKey);
        if (string.IsNullOrEmpty(existingRefreshToken)) 
            await redis.SetValue(refreshKey, refreshToken);

        await redis.SetValue(accessKey, jwtToken, TimeSpan.FromMinutes(jwtService.GetAccessTokenValidity()));
        
        // Note: Change to port 5173 if using Vite default, or 3000 if configured
        return Result<string>.IsSuccess($"http://localhost:5173/google-auth-success?token={jwtToken}"); 
    }

    private async Task<Result<GoogleJsonWebSignature.Payload, string>> ValidateGoogleCodeAsync(string code)
    {
        const string tokenUrl = "https://oauth2.googleapis.com/token";

        if (string.IsNullOrEmpty(_googleOAuthSettings.ClientId))
            return ConfigurationErrors.ClientIdNotFound;
        if (string.IsNullOrEmpty(_googleOAuthSettings.ClientSecret))
            return ConfigurationErrors.ClientSecretNotFound;
        if (string.IsNullOrEmpty(_googleOAuthSettings.RedirectUri))
            return ConfigurationErrors.RedirectUriNotFound;

        var values = new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", _googleOAuthSettings.ClientId },
            { "client_secret", _googleOAuthSettings.ClientSecret },
            { "redirect_uri", _googleOAuthSettings.RedirectUri },
            { "access_type", "offline" },
            { "prompt", "consent" },
            { "grant_type", "authorization_code" }
        };

        try
        {
            var content = new FormUrlEncodedContent(values);
            var response = await httpClient.PostAsync(tokenUrl, content);
            if (!response.IsSuccessStatusCode)
                return GoogleErrors.AuthFailed;

            var responseString = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<GoogleTokenResponse>(responseString);
            if (tokenData?.id_token == null)
                return GoogleErrors.InvalidToken;

            var payload = await GoogleJsonWebSignature.ValidateAsync(tokenData.id_token);
            return Result<GoogleJsonWebSignature.Payload, string>.IsSuccess(payload, tokenData.refresh_token);
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Error validating google code: {ex}");
        }
    }
}