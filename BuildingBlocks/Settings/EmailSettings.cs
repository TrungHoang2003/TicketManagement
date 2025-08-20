namespace BuildingBlocks.EmailHelper;

public class EmailSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Password { get; set; }
    public string Username{ get; set; }
    public bool UseProxy { get; set; } = false;
    public bool UseSystemProxy { get; set; } = false;
    public string ProxyHost { get; set; }
    public int ProxyPort { get; set; }
    public string ProxyUsername { get; set; }
    public string ProxyPassword { get; set; }
}