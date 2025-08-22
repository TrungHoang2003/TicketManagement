using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using BuildingBlocks.Settings;

namespace Application.Services;

public interface IJwtService
{
    int GetUserIdFromToken(string? token);
    string GenerateJwtToken(User user, IList<string> roles);
    string GenerateRefreshToken();
    int GetAccessTokenValidity();
    int GetRefreshTokenValidity();
}

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;

    public JwtService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public int GetUserIdFromToken(string? token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
                throw new ArgumentException("Cant read token or invalid token");

            var jwtToken = handler.ReadJwtToken(token);

            var userIdStr = jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdStr))
                throw new ArgumentException("Cant find userId");

            return int.TryParse(userIdStr, out var id)
                ? id
                : throw new ArgumentException("UserId is not a number");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lấy userId từ token: {ex.Message}");
            throw;
        }
    }

    public string GenerateJwtToken(User user, IList<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? "Email:null"),
            new(JwtRegisteredClaimNames.Name, user.UserName ?? "Username:null"),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

        if (roles.Any())
        {
            foreach (var role in roles.Where(r => !string.IsNullOrWhiteSpace(r)))
                claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
        }

        var accessToken = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenValidityInMinutes),
            claims: claims,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(accessToken);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber).TrimEnd('=');
    }

    public int GetAccessTokenValidity() => _jwtSettings.AccessTokenValidityInMinutes;

    public int GetRefreshTokenValidity() => _jwtSettings.RefreshTokenValidityInDays;
}
