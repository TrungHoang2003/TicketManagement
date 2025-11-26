namespace Application.DTOs;

public class EmailDto
{
    
}

public class SendTicketEmailDto
{
    public  EmailHeader Header { get; set; }
    public int TicketId { get; set; }
    public string CreatorName { get; set; }
    public string ReceiverEmail { get; set; }
    public string ReceiverName { get; set; }
    public string TicketTitle { get; set; }
    public string Priority { get; set; }
    public string? Note { get; set; }
}

public enum EmailHeader
{
    Created = 1, 
    Assigned = 3
}

public class GoogleTokenResponse
{
    public string access_token { get; set; } = string.Empty;
    public string id_token { get; set; } = string.Empty;
    public string refresh_token { get; set; } = string.Empty;
    public int expires_in { get; set; }
    public string scope { get; set; } = string.Empty;
    public string token_type { get; set; } = string.Empty;
}

