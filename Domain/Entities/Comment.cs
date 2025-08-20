using Domain.Interfaces;

namespace Domain.Entities;

public class Comment: Entity
{
    public int TicketId { get; set; }
    
    public Ticket Ticket { get; set; }
    public List<Attachment> Attachments { get; set; }
}