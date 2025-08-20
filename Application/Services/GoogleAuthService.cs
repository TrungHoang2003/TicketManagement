using System.Text.Json;
using Application.Erros;
using Application.Shared;
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

public class GoogleAuthService(IOptions<GoogleOAuthSettings> googleSettings, HttpClient httpClient,
    IUserRepository userRepository, IJwtService jwtService): IGoogleAuthService
{
    private readonly GoogleOAuthSettings _googleSettings = googleSettings.Value;
    public string GetGoogleAuthUrl()
    {
        if (string.IsNullOrEmpty(_googleSettings.ClientId))
            throw new BusinessException("Google Error","Google Client ID not found");

        const string scope = "openid profile email";
        var state = Guid.NewGuid().ToString();

        return $"{_googleSettings.AuthUri}?" +
               $"client_id={_googleSettings.ClientId}&" +
               $"redirect_uri={_googleSettings.RedirectUri}&" +
               $"response_type=code&" +
               $"scope={scope}&" +
               $"access_type=offline&" +
               $"state={state}";
    }

    public async Task<Result<string>> GoogleCallBack(string code)
    {
        var payloadResult = await ValidateGoogleCodeAsync(code);
        if (!payloadResult.Success)
            return payloadResult.Error!;

        var payload = payloadResult.Data;
        
        // Debug logging và null check
        Console.WriteLine($"[DEBUG] Payload Email: {payload.Email}");
        Console.WriteLine($"[DEBUG] Payload Name: {payload.Name}");
        Console.WriteLine($"[DEBUG] Payload Subject: {payload.Subject}");
        Console.WriteLine($"[DEBUG] Payload Audience: {payload.Audience}");
        
        if (string.IsNullOrEmpty(payload.Email))
        {
            return new Error("Google.Auth.NoEmail", "Email not provided by Google. Please ensure email scope is granted.");
        }
        
        var user = await userRepository.FindByEmailAsync(payload.Email);
        
        if (user == null)
        {
            user = new User
            {
                UserName = payload.Email,
                FullName = payload.Name,
                Email = payload.Email,
                AvatarUrl = payload.Picture,
                EmailConfirmed = true // Google users have verified emails
            };

            var result = await userRepository.CreateAsync(user); // No password
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return new Error("Register.Failed", string.Join(",", errors));
            }
            
            await userRepository.AddToRoleAsync(user, "Employee");
        }
        
        var roles = await userRepository.GetRolesAsync(user);
        var jwtToken = jwtService.GenerateJwtToken(user, roles);

        return Result<string>.IsSuccess($"http://localhost:3000/google-auth-success?token={jwtToken}"); 
    }

    private async Task<Result<GoogleJsonWebSignature.Payload>> ValidateGoogleCodeAsync(string code)
    {
        if (string.IsNullOrEmpty(_googleSettings.ClientId))
            return ConfigurationErrors.ClientIdNotFound;
        if (string.IsNullOrEmpty(_googleSettings.ClientSecret))
            return ConfigurationErrors.ClientSecretNotFound;

        var values = new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", _googleSettings.ClientId },
            { "client_secret", _googleSettings.ClientSecret },
            { "redirect_uri", _googleSettings.RedirectUri },
            { "grant_type", "authorization_code" }
        };

        try
        {
            var content = new FormUrlEncodedContent(values);
            var response = await httpClient.PostAsync(_googleSettings.TokenUri, content);
            if (!response.IsSuccessStatusCode)
                return GoogleErrors.AuthFailed;

            var responseString = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<GoogleTokenResponse>(responseString);
            
            // Debug logging
            Console.WriteLine($"[DEBUG] Token response: {responseString}");
            Console.WriteLine($"[DEBUG] Scope received: {tokenData?.scope}");
            
            if (tokenData?.id_token == null)
                return GoogleErrors.InvalidToken;

            var payload = await GoogleJsonWebSignature.ValidateAsync(tokenData.id_token);
            return Result<GoogleJsonWebSignature.Payload>.IsSuccess(payload);
        }
        catch (Exception ex)
        {
            throw new BusinessException("Google Auth Exception", $"Error validating google code: {ex}");
        }
    }
}

public class GoogleTokenResponse
{
    public string access_token { get; set; } = string.Empty;
    public string id_token { get; set; } = string.Empty;
    public string refresh_token { get; set; } = string.Empty;
    public int expires_in { get; set; }
    public string scope { get; set; } = string.Empty;
}