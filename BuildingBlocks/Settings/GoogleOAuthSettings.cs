namespace BuildingBlocks.Settings;

public class GoogleOAuthSettings
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string AuthUri { get; set; }
    public string TokenUri { get; set; }
    public string AuthProviderX509CertUrl { get; set; }
    public string RedirectUri { get; set; }
}
