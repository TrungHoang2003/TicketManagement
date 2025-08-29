using Domain.Interfaces;

namespace Domain.Entities;

public class Progress: Entity
{
    public int TicketId { get; set; }
    public string TicketStatus { get; set; }
    public string Note { get; set;}
    public string EmployeeName { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public Ticket Ticket { get; set; }
}
