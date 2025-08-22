namespace BuildingBlocks.Settings;

public class JwtSettings
{
    public string Secret{ get; set; } = string.Empty;
    public int AccessTokenValidityInMinutes { get; set; }
    public int RefreshTokenValidityInDays { get; set; }
}