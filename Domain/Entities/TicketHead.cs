namespace Domain.Entities;

public class TicketHead
{
    public int TicketId { get; set; }
    public int HeadId{ get; set; }
    public bool IsMainHead { get; set; }
    
    // Navigation properties
    public User Head{ get; set; }
    public Ticket Ticket { get; set; }
}