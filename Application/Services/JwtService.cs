using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Application.Shared;

public interface IJwtService
{
    int GetUserIdFromToken(string? token);
    string GenerateJwtToken(User user, IList<string> roles);
    string GenerateRefreshToken();
    int GetAccessTokenValidity();
    int GetRefreshTokenValidity();
}

public class JwtService(IConfiguration configuration):IJwtService
{
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
            {
                throw new ArgumentException("Cant find userId");
            }

            var userId = int.TryParse(userIdStr, out var id) ? id : throw new ArgumentException("UserId is not a number");

            return userId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lấy userId từ token: {ex.Message}");
            throw;
        }
    }

    public string GenerateJwtToken(User user, IList<string> roles)
    {
        var accesstokenValidity = GetAccessTokenValidity();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"] ?? throw new ArgumentException("Jwt secret not found or not configured in development mode")));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email??"Email:null"),
            new(JwtRegisteredClaimNames.Name, user.UserName??"Username:null"),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

        // Thêm role claims từ IList
        if (roles.Any())
        {
            foreach (var role in roles)
            {
                if (!string.IsNullOrWhiteSpace(role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
                }
            }
        }

        var accessToken = new JwtSecurityToken(
            expires: DateTime.Now.AddMinutes(accesstokenValidity),
            claims: claims,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(accessToken);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var base64String = Convert.ToBase64String(randomNumber);
        return base64String.TrimEnd('=');
    }

    public int GetAccessTokenValidity()
    {
        _ = int.TryParse(configuration["JWT:AccessTokenValidityInMinutes"], out var accessTokenValidity);

        return accessTokenValidity;
    }

    public int GetRefreshTokenValidity()
    {
        _ = int.TryParse(configuration["JWT:RefreshTokenValidityInDays"], out var refreshTokenValidity);

        return refreshTokenValidity;
    }

}
