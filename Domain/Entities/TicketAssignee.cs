namespace Domain.Entities;

public class TicketAssignee
{
    public int TicketId { get; set; }
    public int AssigneeId { get; set; }
    
    // Navigation properties
    public User Assignee { get; set; }
    public Ticket Ticket { get; set; }
}