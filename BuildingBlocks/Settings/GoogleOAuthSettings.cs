namespace BuildingBlocks.Settings;

public class GoogleOAuthSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string ServiceAccountKeyPath { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty; // Email dùng để gửi thay mặt
    public string DisplayName { get; set; } = "🎫 Ticket Management System"; // Tên hiển thị
}
