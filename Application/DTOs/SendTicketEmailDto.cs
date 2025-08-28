namespace Application.DTOs;

public class SendTicketEmailDto
{
    public  EmailHeader Header { get; set; }
    public int TicketId { get; set; }
    public string CreatorName { get; set; }
    public string ReceiverEmail { get; set; }
    public string ReceiverName { get; set; }
    public string TicketTitle { get; set; }
    public string Priority { get; set; }
    public string Reason { get; set; }
}

public enum EmailHeader
{
    Created = 1, 
    Rejected = 2,
    Assigned = 3
}