namespace BuildingBlocks.Settings;

public class EmailSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Password { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FromName { get; set; } = "Ticket Management System";
    public bool UseProxy { get; set; } = false;
    public bool UseSystemProxy { get; set; } = false;
    public string ProxyHost { get; set; } = string.Empty;
    public int ProxyPort { get; set; } = 8080;
    public string ProxyUsername { get; set; } = string.Empty;
    public string ProxyPassword { get; set; } = string.Empty;
}
