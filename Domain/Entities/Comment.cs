using Domain.Interfaces;

namespace Domain.Entities;

public class Comment: Entity
{
    public int TicketId { get; set; }
    public int CreatorId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Ticket Ticket { get; set; }
    public User Creator { get; set; }
    public List<Attachment> Attachments { get; set; }
}